/**
 * chatVoice.js - Web Speech API JS Interop for PDM AI Assistant
 * Provides Speech Recognition (STT) and Speech Synthesis (TTS).
 */

(function () {
    let recognition = null;
    let currentDotNetHelper = null;
    let currentSpeechUtterance = null;

    window.pdmVoice = {
        /**
         * Checks browser support for Speech-to-Text and Text-to-Speech.
         */
        checkSupport: function () {
            const hasSTT = 'webkitSpeechRecognition' in window || 'SpeechRecognition' in window;
            const hasTTS = 'speechSynthesis' in window;
            return { stt: hasSTT, tts: hasTTS };
        },

        /**
         * Starts listening to microphone input.
         * Streams transcribed text back to Blazor component.
         */
        startListening: function (dotNetHelper) {
            currentDotNetHelper = dotNetHelper;

            const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
            if (!SpeechRecognition) {
                if (currentDotNetHelper) {
                    currentDotNetHelper.invokeMethodAsync('OnSpeechError', 'Speech recognition is not supported in this browser.');
                }
                return false;
            }

            if (recognition) {
                try { recognition.stop(); } catch (e) { }
            }

            recognition = new SpeechRecognition();
            recognition.continuous = true;
            recognition.interimResults = true;
            recognition.lang = navigator.language || 'en-US';

            let finalTranscript = '';

            recognition.onstart = function () {
                if (currentDotNetHelper) {
                    currentDotNetHelper.invokeMethodAsync('OnSpeechStarted');
                }
            };

            recognition.onresult = function (event) {
                let interimTranscript = '';
                for (let i = event.resultIndex; i < event.results.length; ++i) {
                    const transcript = event.results[i][0].transcript;
                    if (event.results[i].isFinal) {
                        finalTranscript += transcript + ' ';
                    } else {
                        interimTranscript += transcript;
                    }
                }

                const combinedText = (finalTranscript + interimTranscript).trim();
                if (currentDotNetHelper && combinedText.length > 0) {
                    currentDotNetHelper.invokeMethodAsync('OnSpeechResult', combinedText);
                }
            };

            recognition.onerror = function (event) {
                console.warn('Speech recognition error:', event.error);
                if (currentDotNetHelper) {
                    currentDotNetHelper.invokeMethodAsync('OnSpeechError', event.error);
                }
            };

            recognition.onend = function () {
                if (currentDotNetHelper) {
                    currentDotNetHelper.invokeMethodAsync('OnSpeechEnded');
                }
            };

            try {
                recognition.start();
                return true;
            } catch (err) {
                console.error('Failed to start speech recognition:', err);
                return false;
            }
        },

        /**
         * Stops speech recognition.
         */
        stopListening: function () {
            if (recognition) {
                try {
                    recognition.stop();
                } catch (e) { }
                recognition = null;
            }
        },

        /**
         * Reads the given text out loud using Text-to-Speech (TTS).
         */
        speakText: function (text, messageId, dotNetHelper) {
            if (!('speechSynthesis' in window)) return false;

            // Stop any ongoing speech first
            window.speechSynthesis.cancel();

            if (!text || text.trim().length === 0) return false;

            // Strip Markdown formatting for clean, natural speech
            const cleanText = text
                .replace(/[*_#`~]/g, '')               // remove *, _, #, `, ~
                .replace(/\[([^\]]+)\]\([^)]+\)/g, '$1') // [text](url) -> text
                .replace(/<[^>]+>/g, '')                 // remove HTML tags
                .trim();

            const utterance = new SpeechSynthesisUtterance(cleanText);
            utterance.rate = 1.0;
            utterance.pitch = 1.0;

            // Attempt to select a natural English voice if available
            const voices = window.speechSynthesis.getVoices();
            if (voices && voices.length > 0) {
                const englishVoice = voices.find(v => (v.lang.startsWith('en') && (v.name.includes('Natural') || v.name.includes('Google') || v.name.includes('Samantha') || v.name.includes('Daniel')))) ||
                                     voices.find(v => v.lang.startsWith('en')) ||
                                     voices[0];
                if (englishVoice) {
                    utterance.voice = englishVoice;
                }
            }

            utterance.onend = function () {
                currentSpeechUtterance = null;
                if (dotNetHelper) {
                    dotNetHelper.invokeMethodAsync('OnSpeechFinished', messageId);
                }
            };

            utterance.onerror = function (err) {
                currentSpeechUtterance = null;
                console.warn('Speech synthesis error:', err);
                if (dotNetHelper) {
                    dotNetHelper.invokeMethodAsync('OnSpeechFinished', messageId);
                }
            };

            currentSpeechUtterance = utterance;
            window.speechSynthesis.speak(utterance);
            return true;
        },

        /**
         * Cancels active speech playback.
         */
        stopSpeaking: function () {
            if ('speechSynthesis' in window) {
                window.speechSynthesis.cancel();
                currentSpeechUtterance = null;
            }
        },

        /**
         * Returns true if currently speaking.
         */
        isSpeaking: function () {
            return 'speechSynthesis' in window && window.speechSynthesis.speaking;
        }
    };
})();
