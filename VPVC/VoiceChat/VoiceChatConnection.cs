using System.Collections.Generic;
using System.Linq;
using SIPSorcery;
using SIPSorcery.Net;
using VPVC.BackendCommunication;
using VPVC.BackendCommunication.Shared;
using VPVC.MainInternals;

namespace VPVC.VoiceChat; 

public class VoiceChatConnection {
    private string receivingPartyParticipantId;
    private RTCPeerConnection? peerConnection;
    private NewWindowsAudioEndpoint windowsSourceAudioEndPoint;
    private NewWindowsAudioEndpoint windowsSinkAudioEndPoint;

    public VoiceChatConnection(string receivingPartyParticipantId) {
        this.receivingPartyParticipantId = receivingPartyParticipantId;
        windowsSourceAudioEndPoint = new NewWindowsAudioEndpoint(true, false);
        windowsSinkAudioEndPoint = new NewWindowsAudioEndpoint(false, true);
    }
    
    public void Connect(bool shouldSendOffer) {
        ConnectAsync(shouldSendOffer);
    }
    
    private async void ConnectAsync(bool shouldSendOffer) {
        var configuration = new RTCConfiguration {
            iceServers = new List<RTCIceServer> {
                new RTCIceServer {
                    urls = "stun:176.126.85.128"
                },
                new RTCIceServer {
                    urls = "turn:176.126.85.128",
                    username = "1678386621",
                    credentialType = RTCIceCredentialType.password,
                    credential = "Dvzh2+6i6Mpp9z6KWHFuLWpZQu0="
                }
            },
            iceTransportPolicy = RTCIceTransportPolicy.all
        };
        peerConnection = new RTCPeerConnection(configuration);
        var factory = new WebRtcDebugLoggerFactory();
        LogFactory.Set(factory);

        MediaStreamTrack sendingAudioTrack = new MediaStreamTrack(windowsSourceAudioEndPoint.GetAudioFormat(), MediaStreamStatusEnum.SendOnly);
        peerConnection.addTrack(sendingAudioTrack);
        
        MediaStreamTrack receivingAudioTrack = new MediaStreamTrack(windowsSinkAudioEndPoint.GetAudioFormat(), MediaStreamStatusEnum.RecvOnly);
        peerConnection.addTrack(receivingAudioTrack);

        peerConnection.onconnectionstatechange += connectionState => {
            Logger.Log($"WebRTC peer connection state change to {connectionState}.");

            if (connectionState == RTCPeerConnectionState.connected) {
                windowsSourceAudioEndPoint.StartAudio();
                windowsSinkAudioEndPoint.StartAudio();
            } else if (connectionState == RTCPeerConnectionState.failed) {
                peerConnection.Close("ice disconnection");
                // TODO: Handle disconnect?
            } else if (connectionState == RTCPeerConnectionState.closed) {
                windowsSourceAudioEndPoint.CloseAudio();
                windowsSinkAudioEndPoint.CloseAudio();
            }
        };

        peerConnection.OnRtpPacketReceived += (receivingEndPoint, mediaType, rtpPacket) => {
            if (mediaType == SDPMediaTypesEnum.audio) {
                windowsSinkAudioEndPoint.GotAudioRtp(
                    rtpPacket.Payload
                );
            }
        };
        
        windowsSourceAudioEndPoint.hasNewSamples += samples => peerConnection.SendAudio(1, samples);

        if (!shouldSendOffer) {
            return;
        }

        var sdpOfferSessionDescriptionInit = peerConnection.createOffer();
        await peerConnection.setLocalDescription(sdpOfferSessionDescriptionInit);

        var sdpOfferJson = sdpOfferSessionDescriptionInit.toJSON();
        
        PartyEventSender.SendOutgoingWebRtcSignaling(
            receivingPartyParticipantId,
            VoiceChatSignalingMessageTypes.sdpRemoteDescription,
            sdpOfferJson
        );

        peerConnection.onicecandidate += iceCandidateInit => {
            var iceCandidateJson = iceCandidateInit.toJSON();
            
            Logger.Log($"Sending ice candidate: {iceCandidateJson}");
        
            PartyEventSender.SendOutgoingWebRtcSignaling(
                receivingPartyParticipantId,
                VoiceChatSignalingMessageTypes.iceCandidate,
                iceCandidateJson
            );
        };

        peerConnection.onicecandidateerror += (candidate, s) => {
            Logger.Log($"Ice candidate error ({candidate}, {s})");
        };

        peerConnection.oniceconnectionstatechange += (state) => {
            Logger.Log($"Ice connection state changed to: {state}");
        };
        
        peerConnection.onicegatheringstatechange += (state) => {
            Logger.Log($"Ice gathering state changed to: {state}");
        };
    }

    public void Disconnect() {
        peerConnection?.Close(null);
        peerConnection?.Dispose();
    }

    public void HandleIncomingWebRtcSignaling(string signalingMessageType, string sdpContent) {
        HandleIncomingWebRtcSignalingAsync(signalingMessageType, sdpContent);
    }

    private async void HandleIncomingWebRtcSignalingAsync(string signalingMessageType, string sdpContent) {
        if (peerConnection == null) {
            return;
        }
        
        Logger.Log($"Received signaling message with type: {signalingMessageType} content: {sdpContent}");
        
        if (signalingMessageType == VoiceChatSignalingMessageTypes.sdpRemoteDescription) {
            var decodingSuccess = RTCSessionDescriptionInit.TryParse(sdpContent, out var sdpOfferSessionDescriptionInit);

            if (!decodingSuccess) {
                return;
            }

            var result = peerConnection.setRemoteDescription(sdpOfferSessionDescriptionInit);
        
            Logger.Log($"Result of setting peer connection remote description: {result}");

            if (peerConnection.signalingState == RTCSignalingState.have_remote_offer) {
                var sdpAnswerSessionDescriptionInit = peerConnection.createAnswer();
                await peerConnection.setLocalDescription(sdpAnswerSessionDescriptionInit);

                var sdpAnswerJson = sdpAnswerSessionDescriptionInit.toJSON();
                
                Logger.Log($"Sending SDP answer: {sdpAnswerJson}");
        
                PartyEventSender.SendOutgoingWebRtcSignaling(
                    receivingPartyParticipantId,
                    VoiceChatSignalingMessageTypes.sdpRemoteDescription,
                    sdpAnswerJson
                );
            }
        } else if (signalingMessageType == VoiceChatSignalingMessageTypes.iceCandidate) {
            var decodingSuccess = RTCIceCandidateInit.TryParse(sdpContent, out var iceCandidateInit);
            
            if (!decodingSuccess) {
                Logger.Log("ICE CANDIDATE DECODING FAILED");
                return;
            }
            
            peerConnection.addIceCandidate(iceCandidateInit);
        }
    }

    public void HandlePartyParticipantStateUpdate(Party party, PartyParticipant partyParticipant) {
        // Logger.Log($"Handling participant state update.");
        
        if (partyParticipant.gameState == GameStates.lobby) {
            SetAudioVolume(1f);
        } else if (partyParticipant.gameState == GameStates.agentSelect) {
            SetAudioVolume(
                party.participantSelf.teamIndex == partyParticipant.teamIndex
                    ? 1f
                    : 0f
            );
        } else if (partyParticipant.gameState == GameStates.inGame) {
            var distance = party.participantSelf.CalculateDistanceToOtherParticipant(partyParticipant);

            if (distance < 0) {
                Logger.Log($"Participant distance under 0 (name: {distance}).");
                return;
            }

            if (distance < Config.fullVolumeHearingRadius) {
                SetAudioVolume(1f);
                return;
            }

            if (distance > Config.maxHearingRadius) {
                SetAudioVolume(0f);
                return;
            }

            var distanceOutsideOfFullVolumeRadius = distance - Config.fullVolumeHearingRadius;
            var radiusOutsideOfFullVolumeRadius = Config.maxHearingRadius - Config.fullVolumeHearingRadius;

            var volume = (radiusOutsideOfFullVolumeRadius - distanceOutsideOfFullVolumeRadius) / radiusOutsideOfFullVolumeRadius;
            
            SetAudioVolume((float) volume);
        }
    }

    private void SetAudioVolume(float volumeFraction) {
        var participantDisplayName = PartyManager.currentParty?.otherParticipants.FirstOrDefault(p => p.id == receivingPartyParticipantId)?.userDisplayName;
        // Logger.Log($"Setting participant (name: {participantDisplayName}) volume to {volumeFraction}");
        
        windowsSinkAudioEndPoint.SetOutputVolume(volumeFraction);

        if (volumeFraction == 0f) {
            windowsSourceAudioEndPoint.PauseAudio();
        } else {
            windowsSourceAudioEndPoint.ResumeAudio();
        }
    }
}