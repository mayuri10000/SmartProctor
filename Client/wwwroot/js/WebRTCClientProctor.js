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
    var TestTakerConnection = /** @class */ (function () {
        function TestTakerConnection() {
        }
        return TestTakerConnection;
    }());
    /**
     * Typescript implementation of the WebRTC related functions in the proctor side
     */
    var WebRTCClientProctor = /** @class */ (function () {
        function WebRTCClientProctor() {
            this.testTakerConnections = {};
        }
        WebRTCClientProctor.prototype.init = function (helper, iceServers, testTakers) {
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
                    testTakers.forEach(function (testTaker) {
                        var desktopConnection = new RTCPeerConnection(_this.rtcConfig);
                        var cameraConnection = new RTCPeerConnection(_this.rtcConfig);
                        _this.testTakerConnections[testTaker] = {
                            cameraVideoElem: document.getElementById(testTaker + '-video'),
                            desktopVideoElem: null,
                            desktopConnection: desktopConnection,
                            cameraConnection: cameraConnection,
                            desktopStream: null,
                            cameraStream: null
                        };
                        desktopConnection.ontrack = function (e) { return __awaiter(_this, void 0, void 0, function () {
                            var track;
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0:
                                        track = e.track;
                                        if (this.testTakerConnections[testTaker].desktopVideoElem != null) {
                                            // @ts-ignore
                                            this.testTakerConnections[testTaker].desktopVideoElem.srcObject = e.streams[0];
                                        }
                                        this.testTakerConnections[testTaker].desktopStream = e.streams[0];
                                        return [4 /*yield*/, this.helper.invokeMethodAsync("_onDesktopStream", testTaker)];
                                    case 1:
                                        _a.sent();
                                        return [2 /*return*/];
                                }
                            });
                        }); };
                        desktopConnection.onconnectionstatechange = function (e) { return __awaiter(_this, void 0, void 0, function () {
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0: return [4 /*yield*/, this.helper.invokeMethodAsync("_onDesktopConnectionStateChange", testTaker, desktopConnection.connectionState)];
                                    case 1:
                                        _a.sent();
                                        return [2 /*return*/];
                                }
                            });
                        }); };
                        desktopConnection.onicecandidate = function (e) { return __awaiter(_this, void 0, void 0, function () {
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0: return [4 /*yield*/, this.helper.invokeMethodAsync("_onDesktopIceCandidate", testTaker, e.candidate)];
                                    case 1:
                                        _a.sent();
                                        return [2 /*return*/];
                                }
                            });
                        }); };
                        // @ts-ignore
                        cameraConnection.ontrack = function (e) { return __awaiter(_this, void 0, void 0, function () {
                            var track;
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0:
                                        track = e.track;
                                        if (this.testTakerConnections[testTaker].cameraVideoElem != null) {
                                            // @ts-ignore
                                            this.testTakerConnections[testTaker].cameraVideoElem.srcObject = e.streams[0];
                                        }
                                        this.testTakerConnections[testTaker].cameraStream = e.streams[0];
                                        return [4 /*yield*/, this.helper.invokeMethodAsync("_onCameraStream", testTaker)];
                                    case 1:
                                        _a.sent();
                                        return [2 /*return*/];
                                }
                            });
                        }); };
                        cameraConnection.onconnectionstatechange = function (e) { return __awaiter(_this, void 0, void 0, function () {
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0: return [4 /*yield*/, this.helper.invokeMethodAsync("_onCameraConnectionStateChange", testTaker, cameraConnection.connectionState)];
                                    case 1:
                                        _a.sent();
                                        return [2 /*return*/];
                                }
                            });
                        }); };
                        cameraConnection.onicecandidate = function (e) { return __awaiter(_this, void 0, void 0, function () {
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0: return [4 /*yield*/, this.helper.invokeMethodAsync("_onCameraIceCandidate", testTaker, e.candidate)];
                                    case 1:
                                        _a.sent();
                                        return [2 /*return*/];
                                }
                            });
                        }); };
                    });
                    return [2 /*return*/];
                });
            });
        };
        WebRTCClientProctor.prototype.onReceivedDesktopIceCandidate = function (testTaker, candidate) {
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0: return [4 /*yield*/, this.testTakerConnections[testTaker].desktopConnection.addIceCandidate(candidate)];
                        case 1:
                            _a.sent();
                            return [2 /*return*/];
                    }
                });
            });
        };
        WebRTCClientProctor.prototype.onReceivedDesktopSdp = function (testTaker, sdp) {
            return __awaiter(this, void 0, void 0, function () {
                var conn, answer;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            conn = this.testTakerConnections[testTaker].desktopConnection;
                            return [4 /*yield*/, conn.setRemoteDescription(sdp)];
                        case 1:
                            _a.sent();
                            return [4 /*yield*/, conn.createAnswer()];
                        case 2:
                            answer = _a.sent();
                            return [4 /*yield*/, conn.setLocalDescription(answer)];
                        case 3:
                            _a.sent();
                            return [4 /*yield*/, this.helper.invokeMethodAsync("_onDesktopSdp", testTaker, answer)];
                        case 4:
                            _a.sent();
                            return [2 /*return*/];
                    }
                });
            });
        };
        WebRTCClientProctor.prototype.onReceivedCameraIceCandidate = function (testTaker, candidate) {
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0: return [4 /*yield*/, this.testTakerConnections[testTaker].cameraConnection.addIceCandidate(candidate)];
                        case 1:
                            _a.sent();
                            return [2 /*return*/];
                    }
                });
            });
        };
        WebRTCClientProctor.prototype.onReceivedCameraSdp = function (testTaker, sdp) {
            return __awaiter(this, void 0, void 0, function () {
                var conn, answer;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            conn = this.testTakerConnections[testTaker].cameraConnection;
                            return [4 /*yield*/, conn.setRemoteDescription(sdp)];
                        case 1:
                            _a.sent();
                            return [4 /*yield*/, conn.createAnswer()];
                        case 2:
                            answer = _a.sent();
                            return [4 /*yield*/, conn.setLocalDescription(answer)];
                        case 3:
                            _a.sent();
                            return [4 /*yield*/, this.helper.invokeMethodAsync("_onCameraSdp", testTaker, answer)];
                        case 4:
                            _a.sent();
                            return [2 /*return*/];
                    }
                });
            });
        };
        WebRTCClientProctor.prototype.setDesktopVideoElem = function (testTaker, elementId) {
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    if (this.testTakerConnections[testTaker].desktopVideoElem != null)
                        // @ts-ignore
                        this.testTakerConnections[testTaker].desktopVideoElem.srcObject = null;
                    this.testTakerConnections[testTaker].desktopVideoElem = document.getElementById(elementId);
                    if (this.testTakerConnections[testTaker].desktopVideoElem == null) {
                        return [2 /*return*/];
                    }
                    // @ts-ignore
                    this.testTakerConnections[testTaker].desktopVideoElem.srcObject = this.testTakerConnections[testTaker].desktopStream;
                    return [2 /*return*/];
                });
            });
        };
        WebRTCClientProctor.prototype.setCameraVideoElem = function (testTaker, elementId) {
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    if (this.testTakerConnections[testTaker].cameraVideoElem != null)
                        // @ts-ignore
                        this.testTakerConnections[testTaker].cameraVideoElem.srcObject = null;
                    this.testTakerConnections[testTaker].cameraVideoElem = document.getElementById(elementId);
                    if (this.testTakerConnections[testTaker].cameraVideoElem == null) {
                        return [2 /*return*/];
                    }
                    // @ts-ignore
                    this.testTakerConnections[testTaker].cameraVideoElem.srcObject = this.testTakerConnections[testTaker].cameraStream;
                    return [2 /*return*/];
                });
            });
        };
        return WebRTCClientProctor;
    }());
    SmartProctor.WebRTCClientProctor = WebRTCClientProctor;
})(SmartProctor || (SmartProctor = {}));
var webRTCClientProctor;
export function create(helper, iceServers, testTakers) {
    if (webRTCClientProctor == null) {
        webRTCClientProctor = new SmartProctor.WebRTCClientProctor();
        webRTCClientProctor.init(helper, iceServers, testTakers);
    }
    return webRTCClientProctor;
}
