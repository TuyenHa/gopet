'use strict';

const fs = require('fs');
const path = require('path');
const crypto = require('crypto');
const overrides = require('./overrides.json');

function sha256(bytes) {
    return crypto.createHash('sha256').update(bytes).digest('hex');
}

function rawPngBytes(relativePath, source) {
    const key = relativePath.replace(/\\/g, '/');
    const override = overrides[key];
    if (!override) return source;
    if (sha256(source) !== override.sourceSha256) {
        throw new Error(`Raw PNG source changed; review override: ${key}`);
    }
    const bytes = fs.readFileSync(path.join(__dirname, 'overrides', key));
    if (sha256(bytes) !== override.overrideSha256) {
        throw new Error(`Raw PNG override changed; update its manifest after review: ${key}`);
    }
    return bytes;
}

module.exports = { rawPngBytes };
