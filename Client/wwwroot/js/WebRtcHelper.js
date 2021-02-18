WebRtcApp = WebRtcApp || {}

WebRtcApp.Helper = (function (connectionManager) {
    var _mediaStream, _dotnet,
        _init = function (dotnet) {
            _dotnet = dotnet;
        },
        
        _openDesktopStream = function () {
            var success = false;
            navigator.mediaDevices.getDisplayMedia().then(function (stream) {
                console.log('initializing connection manager');
                connectionManager.initialize(_callbacks.sendSignal, _callbacks.onReadyForStream,
                    _callbacks.onStreamAdded, _callbacks.onStreamRemoved);
                _mediaStream = stream;

                var videoElem = document.querySelector('.video.desktop');
                attachMediaStream(videoElem, _mediaStream);
                success = true;
            }).catch(function (e) {
                console.error('cannot open display capture', e);
            });
            return success;
        },
        
        _startStreamToProctor = function(connId) {
            connectionManager.initiateOffer(connId, _mediaStream);
        },

        // Connection Manager Callbacks
        _callbacks = {
            sendSignal: function (msg, cid) {
                _dotnet.invokeMethodAsync("sendSignal", msg, cid);
            },

            onReadyForStream: function (connection) {
                // The connection manager needs our stream
                // todo: not sure I like this
                connection.addStream(_mediaStream);
            },
            onStreamAdded: function (connection, event) {
                console.log('binding remote stream to the partner window');

                // Bind the remote stream to the partner window
                var otherVideo = document.querySelector('.video.taker-' + connection);
                otherVideo.srcObject = event.stream;
                _dotnet.invokeMethodAsync("examTakerConnected", connection);
            },
            onStreamRemoved: function (connection, streamId) {
                // todo: proper stream removal.  right now we are only set up for one-on-one which is why this works.
                console.log('removing remote stream from partner window');

                // Clear out the partner window
                var otherVideo = document.querySelector('.video.taker-' + connection);
                otherVideo.src = '';
                _dotnet.invokeMethodAsync("examTakerDisconnected", connection);
            }
        };

    return {
        init: _init,
        openDesktopStream: _openDesktopStream,
        startStreamToProctor: _startStreamToProctor,
        getStream: function () { // Temp hack for the connection manager to reach back in here for a stream
            return _mediaStream;
        }
    };
})(WebRtcApp.ConnectionManager);