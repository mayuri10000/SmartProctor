var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
var SmartProctor;
(function (SmartProctor) {
    var ProctorConnection = /** @class */ (function () {
        function ProctorConnection() {
        }
        return ProctorConnection;
    }());
    /**
     * Typescript implementation of the WebRTC related functions in the test-taker side
     */
    var WebRTCClientTaker = /** @class */ (function () {
        function WebRTCClientTaker() {
            this.proctorConnections = {};
        }
        WebRTCClientTaker.prototype.init = function (helper, iceServers, proctors) {
            return __awaiter(this, void 0, void 0, function () {
                var _this = this;
                return __generator(this, function (_a) {
                    this.helper = helper;
                    if (iceServers != null) {
                        this.rtcConfig = { iceServers: [{ urls: iceServers }] };
                    }
                    else {
                        this.rtcConfig = null;
                    }
                    proctors.forEach(function (proctor) {
                        var desktopConn = new RTCPeerConnection(_this.rtcConfig);
                        var cameraConn = new RTCPeerConnection(_this.rtcConfig);
                        desktopConn.onicecandidate = function (e) { return __awaiter(_this, void 0, void 0, function () {
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0: return [4 /*yield*/, helper.invokeMethodAsync("_onDesktopIceCandidate", proctor, e.candidate)];
                                    case 1:
                                        _a.sent();
                                        return [2 /*return*/];
                                }
                            });
                        }); };
                        desktopConn.onconnectionstatechange = function (e) { return __awaiter(_this, void 0, void 0, function () {
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0: return [4 /*yield*/, helper.invokeMethodAsync("_onDesktopConnectionStateChange", proctor, desktopConn.connectionState)];
                                    case 1:
                                        _a.sent();
                                        return [2 /*return*/];
                                }
                            });
                        }); };
                        cameraConn.onicecandidate = function (e) { return __awaiter(_this, void 0, void 0, function () {
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0: return [4 /*yield*/, helper.invokeMethodAsync("_onCameraIceCandidate", proctor, e.candidate)];
                                    case 1:
                                        _a.sent();
                                        return [2 /*return*/];
                                }
                            });
                        }); };
                        cameraConn.onconnectionstatechange = function (e) { return __awaiter(_this, void 0, void 0, function () {
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0: return [4 /*yield*/, helper.invokeMethodAsync("_onCameraConnectionStateChange", proctor, cameraConn.connectionState)];
                                    case 1:
                                        _a.sent();
                                        return [2 /*return*/];
                                }
                            });
                        }); };
                        _this.proctorConnections[proctor] = {
                            desktopConnection: desktopConn,
                            cameraConnection: cameraConn
                        };
                    });
                    return [2 /*return*/];
                });
            });
        };
        WebRTCClientTaker.prototype.obtainDesktopStream = function () {
            return __awaiter(this, void 0, void 0, function () {
                var _a;
                var _this = this;
                return __generator(this, function (_b) {
                    switch (_b.label) {
                        case 0:
                            // @ts-ignore
                            _a = this;
                            return [4 /*yield*/, navigator.mediaDevices.getDisplayMedia()];
                        case 1:
                            // @ts-ignore
                            _a.desktopStream = _b.sent();
                            // @ts-ignore
                            this.desktopStream.oninactive = function (_) { return __awaiter(_this, void 0, void 0, function () {
                                return __generator(this, function (_a) {
                                    switch (_a.label) {
                                        case 0: return [4 /*yield*/, this.helper.invokeMethodAsync("_onDesktopInactivated")];
                                        case 1:
                                            _a.sent();
                                            return [2 /*return*/];
                                    }
                                });
                            }); };
                            return [2 /*return*/, this.desktopStream.getTracks()[0].label];
                    }
                });
            });
        };
        WebRTCClientTaker.prototype.obtainCameraStream = function (mjpegUrl) {
            return __awaiter(this, void 0, void 0, function () {
                var _this = this;
                return __generator(this, function (_a) {
                    if (this.cameraCanvas == null) {
                        // @ts-ignore
                        this.cameraCanvas = document.createElement("canvas");
                        this.cameraCanvas.width = 858;
                        this.cameraCanvas.height = 480;
                    }
                    if (this.cameraImage == null) {
                        this.cameraImage = new Image();
                        this.cameraImage.crossOrigin = "anonymous";
                    }
                    this.cameraImage.src = mjpegUrl;
                    // @ts-ignore
                    this.cameraStream = this.cameraCanvas.captureStream();
                    // @ts-ignore
                    this.cameraStream.oninactive = function (_) { return __awaiter(_this, void 0, void 0, function () {
                        return __generator(this, function (_a) {
                            switch (_a.label) {
                                case 0: return [4 /*yield*/, this.helper.invokeMethodAsync("_onCameraInactivated")];
                                case 1:
                                    _a.sent();
                                    return [2 /*return*/];
                            }
                        });
                    }); };
                    window.setInterval(function () {
                        _this.cameraCanvas.getContext("2d").drawImage(_this.cameraImage, 0, 0);
                    }, 15);
                    return [2 /*return*/];
                });
            });
        };
        WebRTCClientTaker.prototype.startStreaming = function () {
            return __awaiter(this, void 0, void 0, function () {
                var _loop_1, this_1, _a, _b, _i, proctor;
                var _this = this;
                return __generator(this, function (_c) {
                    switch (_c.label) {
                        case 0:
                            _loop_1 = function (proctor) {
                                var conn, desktopOffer, cameraOffer;
                                return __generator(this, function (_d) {
                                    switch (_d.label) {
                                        case 0:
                                            conn = this_1.proctorConnections[proctor];
                                            this_1.desktopStream.getTracks().forEach(function (track) {
                                                conn.desktopConnection.addTrack(track, _this.desktopStream);
                                            });
                                            this_1.cameraStream.getTracks().forEach(function (track) {
                                                conn.cameraConnection.addTrack(track, _this.cameraStream);
                                            });
                                            return [4 /*yield*/, conn.desktopConnection.createOffer()];
                                        case 1:
                                            desktopOffer = _d.sent();
                                            return [4 /*yield*/, conn.desktopConnection.setLocalDescription(desktopOffer)];
                                        case 2:
                                            _d.sent();
                                            return [4 /*yield*/, conn.cameraConnection.createOffer()];
                                        case 3:
                                            cameraOffer = _d.sent();
                                            return [4 /*yield*/, conn.cameraConnection.setLocalDescription(cameraOffer)
                                                // Send the local SDP through SignalR in .NET
                                            ];
                                        case 4:
                                            _d.sent();
                                            // Send the local SDP through SignalR in .NET
                                            return [4 /*yield*/, this_1.helper.invokeMethodAsync("_onCameraSdp", proctor, cameraOffer)];
                                        case 5:
                                            // Send the local SDP through SignalR in .NET
                                            _d.sent();
                                            return [4 /*yield*/, this_1.helper.invokeMethodAsync("_onDesktopSdp", proctor, desktopOffer)];
                                        case 6:
                                            _d.sent();
                                            return [2 /*return*/];
                                    }
                                });
                            };
                            this_1 = this;
                            _a = [];
                            for (_b in this.proctorConnections)
                                _a.push(_b);
                            _i = 0;
                            _c.label = 1;
                        case 1:
                            if (!(_i < _a.length)) return [3 /*break*/, 4];
                            proctor = _a[_i];
                            return [5 /*yield**/, _loop_1(proctor)];
                        case 2:
                            _c.sent();
                            _c.label = 3;
                        case 3:
                            _i++;
                            return [3 /*break*/, 1];
                        case 4: return [2 /*return*/];
                    }
                });
            });
        };
        WebRTCClientTaker.prototype.reconnectToProctor = function (proctor) {
            return __awaiter(this, void 0, void 0, function () {
                var conn, desktopOffer, cameraOffer;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            conn = this.proctorConnections[proctor];
                            return [4 /*yield*/, conn.desktopConnection.createOffer()];
                        case 1:
                            desktopOffer = _a.sent();
                            return [4 /*yield*/, conn.desktopConnection.setLocalDescription(desktopOffer)];
                        case 2:
                            _a.sent();
                            return [4 /*yield*/, conn.cameraConnection.createOffer()];
                        case 3:
                            cameraOffer = _a.sent();
                            return [4 /*yield*/, conn.cameraConnection.setLocalDescription(cameraOffer)];
                        case 4:
                            _a.sent();
                            // Send the local SDP through SignalR in .NET
                            return [4 /*yield*/, this.helper.invokeMethodAsync("_onDesktopSdp", proctor, desktopOffer)];
                        case 5:
                            // Send the local SDP through SignalR in .NET
                            _a.sent();
                            return [4 /*yield*/, this.helper.invokeMethodAsync("_onCameraSdp", proctor, cameraOffer)];
                        case 6:
                            _a.sent();
                            return [2 /*return*/];
                    }
                });
            });
        };
        WebRTCClientTaker.prototype.setDesktopVideoElement = function (elementId) {
            if (this.desktopVideoElem != null)
                // @ts-ignore
                this.desktopVideoElem.srcObject = null;
            this.desktopVideoElem = document.getElementById(elementId);
            // @ts-ignore
            this.desktopVideoElem.srcObject = this.desktopStream;
        };
        WebRTCClientTaker.prototype.setCameraVideoElement = function (elementId) {
            if (this.cameraVideoElem != null)
                // @ts-ignore
                this.cameraVideoElem.srcObject = null;
            this.cameraVideoElem = document.getElementById(elementId);
            // @ts-ignore
            this.cameraVideoElem.srcObject = this.cameraStream;
        };
        WebRTCClientTaker.prototype.receivedDesktopAnswerSDP = function (proctor, sdp) {
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0: return [4 /*yield*/, this.proctorConnections[proctor].desktopConnection.setRemoteDescription(sdp)];
                        case 1:
                            _a.sent();
                            return [2 /*return*/];
                    }
                });
            });
        };
        WebRTCClientTaker.prototype.receivedDesktopIceCandidate = function (proctor, candidate) {
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0: return [4 /*yield*/, this.proctorConnections[proctor].desktopConnection.addIceCandidate(candidate)];
                        case 1:
                            _a.sent();
                            return [2 /*return*/];
                    }
                });
            });
        };
        WebRTCClientTaker.prototype.receivedCameraAnswerSDP = function (proctor, sdp) {
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0: return [4 /*yield*/, this.proctorConnections[proctor].cameraConnection.setRemoteDescription(sdp)];
                        case 1:
                            _a.sent();
                            return [2 /*return*/];
                    }
                });
            });
        };
        WebRTCClientTaker.prototype.receivedCameraIceCandidate = function (proctor, candidate) {
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0: return [4 /*yield*/, this.proctorConnections[proctor].cameraConnection.addIceCandidate(candidate)];
                        case 1:
                            _a.sent();
                            return [2 /*return*/];
                    }
                });
            });
        };
        WebRTCClientTaker.prototype.onProctorReconnected = function (proctor) {
            return __awaiter(this, void 0, void 0, function () {
                var conn, desktopOffer, cameraOffer;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            conn = this.proctorConnections[proctor];
                            return [4 /*yield*/, conn.desktopConnection.createOffer()];
                        case 1:
                            desktopOffer = _a.sent();
                            return [4 /*yield*/, conn.desktopConnection.setLocalDescription(desktopOffer)];
                        case 2:
                            _a.sent();
                            return [4 /*yield*/, conn.cameraConnection.createOffer()];
                        case 3:
                            cameraOffer = _a.sent();
                            return [4 /*yield*/, conn.cameraConnection.setLocalDescription(cameraOffer)];
                        case 4:
                            _a.sent();
                            // Send the local SDP through SignalR in .NET
                            return [4 /*yield*/, this.helper.invokeMethodAsync("_onDesktopSdp", proctor, desktopOffer)];
                        case 5:
                            // Send the local SDP through SignalR in .NET
                            _a.sent();
                            return [4 /*yield*/, this.helper.invokeMethodAsync("_onCameraSdp", proctor, cameraOffer)];
                        case 6:
                            _a.sent();
                            return [2 /*return*/];
                    }
                });
            });
        };
        return WebRTCClientTaker;
    }());
    SmartProctor.WebRTCClientTaker = WebRTCClientTaker;
})(SmartProctor || (SmartProctor = {}));
var webRTCClientTaker;
export function create(helper, iceServers, proctors) {
    if (webRTCClientTaker == null) {
        webRTCClientTaker = new SmartProctor.WebRTCClientTaker();
        webRTCClientTaker.init(helper, iceServers, proctors);
    }
    return webRTCClientTaker;
}
