/* 
 * Passkey Utilities for MoreSpeakers.com
 * Handles WebAuthn registration and authentication
 */

const Passkeys = {
    // Generate friendly name based on browser and OS (like GitHub)
    generateFriendlyName: function () {
        const ua = navigator.userAgent;
        let browser = 'Browser';
        let os = 'Unknown';

        // Detect browser
        if (ua.indexOf('Edg') > -1) {
            browser = 'Edge';
        } else if (ua.indexOf('Chrome') > -1) {
            browser = 'Chrome';
        } else if (ua.indexOf('Safari') > -1) {
            browser = 'Safari';
        } else if (ua.indexOf('Firefox') > -1) {
            browser = 'Firefox';
        }

        // Detect OS
        if (ua.indexOf('Win') > -1) {
            os = 'Windows';
        } else if (ua.indexOf('Mac') > -1) {
            os = ua.indexOf('iPhone') > -1 || ua.indexOf('iPad') > -1 ? 'iOS' : 'macOS';
        } else if (ua.indexOf('Android') > -1) {
            os = 'Android';
        } else if (ua.indexOf('Linux') > -1) {
            os = 'Linux';
        }

        const timestamp = new Date().toLocaleString('en-US', {
            month: 'short',
            day: 'numeric',
            year: 'numeric'
        });

        return `${browser} on ${os} (${timestamp})`;
    },

    // Helper to convert array buffer to base64url
    convertToBase64: function (o) {
        if (!o) {
            return undefined;
        }

        // Normalize Array to Uint8Array
        if (Array.isArray(o)) {
            o = Uint8Array.from(o);
        }

        // Normalize ArrayBuffer to Uint8Array
        if (o instanceof ArrayBuffer) {
            o = new Uint8Array(o);
        }

        // Convert Uint8Array to base64
        if (o instanceof Uint8Array) {
            let str = '';
            for (let i = 0; i < o.byteLength; i++) {
                str += String.fromCharCode(o[i]);
            }
            o = window.btoa(str);
        }

        if (typeof o !== 'string') {
            throw new Error("Could not convert to base64 string");
        }

        // Convert base64 to base64url
        o = o.replace(/\+/g, "-").replace(/\//g, "_").replace(/=*$/g, "");

        return o;
    },

    // Register a new passkey
    // Returns a Promise that resolves on success or rejects with an Error
    register: async function (friendlyName) {
        // Auto-generate friendly name if not provided
        if (!friendlyName || friendlyName.trim() === '') {
            friendlyName = this.generateFriendlyName();
        }

        // 1. Request options
        const response = await fetch('/api/Passkey/creationOptions', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) throw new Error('Failed to get creation options from server.');

        const optionsJson = await response.json();
        
        // 2. Create credential using browser API
        let options;
        if (typeof PublicKeyCredential.parseCreationOptionsFromJSON === 'function') {
            options = PublicKeyCredential.parseCreationOptionsFromJSON(optionsJson);
        } else {
            throw new Error("Your browser doesn't support modern WebAuthn JSON parsing. Please update your browser.");
        }

        const credential = await navigator.credentials.create({ publicKey: options });

        // 3. Prepare response for server
        // Manual serialization to avoid "Illegal invocation" error in some browsers
        const credentialJson = JSON.stringify({
            authenticatorAttachment: credential.authenticatorAttachment,
            clientExtensionResults: credential.getClientExtensionResults(),
            id: credential.id,
            rawId: this.convertToBase64(credential.rawId),
            response: {
                attestationObject: this.convertToBase64(credential.response.attestationObject),
                authenticatorData: this.convertToBase64(credential.response.authenticatorData ?? 
                  credential.response.getAuthenticatorData?.() ?? undefined),
                clientDataJSON: this.convertToBase64(credential.response.clientDataJSON),
                publicKey: this.convertToBase64(credential.response.getPublicKey?.() ?? undefined),
                publicKeyAlgorithm: credential.response.getPublicKeyAlgorithm?.() ?? undefined,
                transports: credential.response.getTransports?.() ?? undefined,
                signature: this.convertToBase64(credential.response.signature),
                userHandle: this.convertToBase64(credential.response.userHandle),
            },
            type: credential.type,
        });

        // 4. Send to server
        const regResponse = await fetch('/api/Passkey/register', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                credentialJson: credentialJson,
                friendlyName: friendlyName
            })
        });

        if (!regResponse.ok) {
            const errText = await regResponse.text();
            throw new Error(errText || 'Failed to register passkey.');
        }

        return true;
    },

    // Login with passkey
    // Returns a Promise that resolves on success or rejects with an Error
    login: async function (email) {
        const url = email 
            ? `/api/Passkey/loginOptions?email=${encodeURIComponent(email)}` 
            : '/api/Passkey/loginOptions';

        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) throw new Error('Failed to get login options from server.');

        const optionsJson = await response.json();
        let options;
        
        if (typeof PublicKeyCredential.parseRequestOptionsFromJSON === 'function') {
            options = PublicKeyCredential.parseRequestOptionsFromJSON(optionsJson);
        } else {
                throw new Error("Your browser doesn't support modern WebAuthn JSON parsing.");
        }

        // This will throw if user cancels
        const credential = await navigator.credentials.get({ publicKey: options });

        // Serialize assertion
        const credentialJson = JSON.stringify({
            authenticatorAttachment: credential.authenticatorAttachment,
            clientExtensionResults: credential.getClientExtensionResults(),
            id: credential.id,
            rawId: this.convertToBase64(credential.rawId),
            response: {
                authenticatorData: this.convertToBase64(credential.response.authenticatorData),
                clientDataJSON: this.convertToBase64(credential.response.clientDataJSON),
                signature: this.convertToBase64(credential.response.signature),
                userHandle: this.convertToBase64(credential.response.userHandle)
            },
            type: credential.type
        });

        const loginResponse = await fetch('/api/Passkey/login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ credentialJson: credentialJson })
        });

        if (!loginResponse.ok) {
            const errorText = await loginResponse.text();
            throw new Error(errorText || 'Authentication failed.');
        }

        return true;
    },

    // Delete a passkey
    delete: async function(id) {
        const response = await fetch(`/api/Passkey/${id}`, {
            method: 'DELETE'
        });

        if (!response.ok) {
            throw new Error('Failed to delete passkey.');
        }

        return true;
    }
};

window.Passkeys = Passkeys;