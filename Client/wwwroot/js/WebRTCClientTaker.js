"use strict";
var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
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
Object.defineProperty(exports, "__esModule", { value: true });
exports.create = void 0;
// @ts-ignore
window.SmartProctor = window.SmartProctor || {};
var SmartProctor;
(function (SmartProctor) {
    /**
     * Typescript implementation of the WebRTC related functions in the test-taker side
     */
    var WebRTCClientTaker = /** @class */ (function (_super) {
        __extends(WebRTCClientTaker, _super);
        function WebRTCClientTaker() {
            return _super !== null && _super.apply(this, arguments) || this;
        }
        WebRTCClientTaker.prototype.init = function (helper) {
            return __awaiter(this, void 0, void 0, function () {
                var _this = this;
                return __generator(this, function (_a) {
                    this.helper = helper;
                    this.connection = new RTCPeerConnection();
                    this.connection.addTransceiver("video", { direction: 'sendonly' });
                    this.connection.onicecandidate = function (e) { return __awaiter(_this, void 0, void 0, function () {
                        return __generator(this, function (_a) {
                            switch (_a.label) {
                                case 0: return [4 /*yield*/, helper.invokeMethodAsync("_onIceCandidate", e.candidate)];
                                case 1:
                                    _a.sent();
                                    return [2 /*return*/];
                            }
                        });
                    }); };
                    this.connection.oniceconnectionstatechange = function (e) { return __awaiter(_this, void 0, void 0, function () {
                        return __generator(this, function (_a) {
                            switch (_a.label) {
                                case 0: return [4 /*yield*/, helper.invokeMethodAsync("_onIceConnectionStateChange", this.connection.iceConnectionState)];
                                case 1:
                                    _a.sent();
                                    return [2 /*return*/];
                            }
                        });
                    }); };
                    this.connection.onconnectionstatechange = function (e) { return __awaiter(_this, void 0, void 0, function () {
                        return __generator(this, function (_a) {
                            switch (_a.label) {
                                case 0: return [4 /*yield*/, helper.invokeMethodAsync("_onConnectionStateChange", this.connection.connectionState)];
                                case 1:
                                    _a.sent();
                                    return [2 /*return*/];
                            }
                        });
                    }); };
                    return [2 /*return*/];
                });
            });
        };
        WebRTCClientTaker.prototype.startStreaming = function () {
            return __awaiter(this, void 0, void 0, function () {
                var _a, localVideo, offer;
                var _this = this;
                return __generator(this, function (_b) {
                    switch (_b.label) {
                        case 0:
                            // @ts-ignore
                            _a = this;
                            return [4 /*yield*/, navigator.mediaDevices.getDisplayMedia()];
                        case 1:
                            // @ts-ignore
                            _a.stream = _b.sent();
                            localVideo = document.querySelector(".video.local");
                            // @ts-ignore
                            localVideo.srcObject = this.stream;
                            this.stream.getTracks().forEach(function (track) {
                                _this.connection.addTrack(track, _this.stream);
                            });
                            return [4 /*yield*/, this.connection.createOffer()];
                        case 2:
                            offer = _b.sent();
                            return [4 /*yield*/, this.connection.setLocalDescription(offer)];
                        case 3:
                            _b.sent();
                            // Send the local SDP through SignalR in .NET
                            return [4 /*yield*/, this.helper.invokeMethodAsync("_onLocalSdp", offer)];
                        case 4:
                            // Send the local SDP through SignalR in .NET
                            _b.sent();
                            return [2 /*return*/];
                    }
                });
            });
        };
        WebRTCClientTaker.prototype.receivedAnswerSDP = function (sdp) {
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0: return [4 /*yield*/, this.connection.setRemoteDescription(sdp)];
                        case 1:
                            _a.sent();
                            return [2 /*return*/];
                    }
                });
            });
        };
        WebRTCClientTaker.prototype.receivedIceCandidate = function (candidate) {
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0: return [4 /*yield*/, this.connection.addIceCandidate(candidate)];
                        case 1:
                            _a.sent();
                            return [2 /*return*/];
                    }
                });
            });
        };
        WebRTCClientTaker.prototype.getIceConnectionState = function () {
            return this.connection.iceConnectionState;
        };
        WebRTCClientTaker.prototype.getConnectionState = function () {
            return this.connection.connectionState;
        };
        return WebRTCClientTaker;
    }(EventTarget));
    SmartProctor.WebRTCClientTaker = WebRTCClientTaker;
})(SmartProctor || (SmartProctor = {}));
var webRTCClientTaker;
function create(helper) {
    if (webRTCClientTaker == null) {
        webRTCClientTaker = new SmartProctor.WebRTCClientTaker();
        webRTCClientTaker.init(helper);
    }
    return webRTCClientTaker;
}
exports.create = create;
//# sourceMappingURL=WebRTCClientTaker.js.map