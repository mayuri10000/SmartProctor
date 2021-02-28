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
    /**
     * Typescript implementation of the WebRTC related functions in the test-taker side
     */
    var WebRTCClientTaker = /** @class */ (function () {
        function WebRTCClientTaker() {
            this.proctorConnections = {};
        }
        WebRTCClientTaker.prototype.init = function (helper, proctors) {
            return __awaiter(this, void 0, void 0, function () {
                var _this = this;
                return __generator(this, function (_a) {
                    this.helper = helper;
                    this.cameraConnection = new RTCPeerConnection();
                    this.cameraConnection.onicecandidate = function (e) { return __awaiter(_this, void 0, void 0, function () {
                        return __generator(this, function (_a) {
                            switch (_a.label) {
                                case 0: return [4 /*yield*/, helper.invokeMethodAsync("_onCameraIceCandidate", e.candidate)];
                                case 1:
                                    _a.sent();
                                    return [2 /*return*/];
                            }
                        });
                    }); };
                    this.cameraConnection.onconnectionstatechange = function (e) { return __awaiter(_this, void 0, void 0, function () {
                        return __generator(this, function (_a) {
                            switch (_a.label) {
                                case 0: return [4 /*yield*/, helper.invokeMethodAsync("_onCameraConnectionStateChange", this.cameraConnection.connectionState)];
                                case 1:
                                    _a.sent();
                                    return [2 /*return*/];
                            }
                        });
                    }); };
                    this.cameraConnection.ontrack = function (e) { return __awaiter(_this, void 0, void 0, function () {
                        return __generator(this, function (_a) {
                            this.cameraStream = e.streams[0];
                            return [2 /*return*/];
                        });
                    }); };
                    proctors.forEach(function (proctor) {
                        var conn = new RTCPeerConnection(null);
                        conn.onicecandidate = function (e) { return __awaiter(_this, void 0, void 0, function () {
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0:
                                        console.log("Sending ICE candidate to " + proctor + ".");
                                        return [4 /*yield*/, helper.invokeMethodAsync("_onProctorIceCandidate", proctor, e.candidate)];
                                    case 1:
                                        _a.sent();
                                        return [2 /*return*/];
                                }
                            });
                        }); };
                        conn.onconnectionstatechange = function (e) { return __awaiter(_this, void 0, void 0, function () {
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0:
                                        console.log("connection state of " + proctors + " changed to '" + conn.connectionState + "'");
                                        return [4 /*yield*/, helper.invokeMethodAsync("_onProctorConnectionStateChange", proctor, conn.connectionState)];
                                    case 1:
                                        _a.sent();
                                        return [2 /*return*/];
                                }
                            });
                        }); };
                        _this.proctorConnections[proctor] = conn;
                    });
                    return [2 /*return*/];
                });
            });
        };
        WebRTCClientTaker.prototype.startStreamingDesktop = function () {
            return __awaiter(this, void 0, void 0, function () {
                var _a, _loop_1, this_1, _b, _c, _i, proctor;
                var _this = this;
                return __generator(this, function (_d) {
                    switch (_d.label) {
                        case 0:
                            // @ts-ignore
                            _a = this;
                            return [4 /*yield*/, navigator.mediaDevices.getDisplayMedia()];
                        case 1:
                            // @ts-ignore
                            _a.desktopStream = _d.sent();
                            _loop_1 = function (proctor) {
                                var conn, offer;
                                return __generator(this, function (_a) {
                                    switch (_a.label) {
                                        case 0:
                                            conn = this_1.proctorConnections[proctor];
                                            this_1.desktopStream.getTracks().forEach(function (track) {
                                                conn.addTrack(track, _this.desktopStream);
                                            });
                                            return [4 /*yield*/, conn.createOffer()];
                                        case 1:
                                            offer = _a.sent();
                                            return [4 /*yield*/, conn.setLocalDescription(offer)];
                                        case 2:
                                            _a.sent();
                                            // Send the local SDP through SignalR in .NET
                                            return [4 /*yield*/, this_1.helper.invokeMethodAsync("_onProctorSdp", proctor, offer)];
                                        case 3:
                                            // Send the local SDP through SignalR in .NET
                                            _a.sent();
                                            console.log("Sending offer to " + proctor + ".");
                                            return [2 /*return*/];
                                    }
                                });
                            };
                            this_1 = this;
                            _b = [];
                            for (_c in this.proctorConnections)
                                _b.push(_c);
                            _i = 0;
                            _d.label = 2;
                        case 2:
                            if (!(_i < _b.length)) return [3 /*break*/, 5];
                            proctor = _b[_i];
                            return [5 /*yield**/, _loop_1(proctor)];
                        case 3:
                            _d.sent();
                            _d.label = 4;
                        case 4:
                            _i++;
                            return [3 /*break*/, 2];
                        case 5: return [2 /*return*/];
                    }
                });
            });
        };
        WebRTCClientTaker.prototype.setDesktopVideoElement = function (elementId) {
            var localVideo = document.getElementById(elementId);
            // @ts-ignore
            localVideo.srcObject = this.desktopStream;
        };
        WebRTCClientTaker.prototype.setCameraVideoElement = function (elementId) {
            var localVideo = document.getElementById(elementId);
            // @ts-ignore
            localVideo.srcObject = this.cameraStream;
        };
        WebRTCClientTaker.prototype.receivedProctorAnswerSDP = function (proctor, sdp) {
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0: return [4 /*yield*/, this.proctorConnections[proctor].setRemoteDescription(sdp)];
                        case 1:
                            _a.sent();
                            console.log("received answer from " + proctor + " and sending answer.");
                            return [2 /*return*/];
                    }
                });
            });
        };
        WebRTCClientTaker.prototype.receivedProctorIceCandidate = function (proctor, candidate) {
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0: return [4 /*yield*/, this.proctorConnections[proctor].addIceCandidate(candidate)];
                        case 1:
                            _a.sent();
                            console.log("received ICE candidate from " + proctor + ".");
                            return [2 /*return*/];
                    }
                });
            });
        };
        WebRTCClientTaker.prototype.receivedCameraOfferSDP = function (sdp) {
            return __awaiter(this, void 0, void 0, function () {
                var answer;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0: return [4 /*yield*/, this.cameraConnection.setRemoteDescription(sdp)];
                        case 1:
                            _a.sent();
                            return [4 /*yield*/, this.cameraConnection.createAnswer()];
                        case 2:
                            answer = _a.sent();
                            return [4 /*yield*/, this.cameraConnection.setLocalDescription(answer)];
                        case 3:
                            _a.sent();
                            return [4 /*yield*/, this.helper.invokeMethodAsync("_onCameraSdp", answer)];
                        case 4:
                            _a.sent();
                            return [2 /*return*/];
                    }
                });
            });
        };
        WebRTCClientTaker.prototype.receivedCameraIceCandidate = function (candidate) {
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0: return [4 /*yield*/, this.cameraConnection.addIceCandidate(candidate)];
                        case 1:
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
export function create(helper, proctors) {
    if (webRTCClientTaker == null) {
        webRTCClientTaker = new SmartProctor.WebRTCClientTaker();
        webRTCClientTaker.init(helper, proctors);
    }
    return webRTCClientTaker;
}
//# sourceMappingURL=WebRTCClientTaker.js.map