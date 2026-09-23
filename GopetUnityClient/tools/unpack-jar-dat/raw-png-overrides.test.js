'use strict';

const assert = require('node:assert/strict');
const fs = require('fs');
const path = require('path');
const { rawPngBytes } = require('./raw-png-overrides');

const original = fs.readFileSync(path.resolve(__dirname,
    '../../../client.jar_Decompiler.com/newMapData/158.png'));
const customized = fs.readFileSync(path.join(__dirname, 'overrides/newMapData/158.png'));
assert.notDeepEqual(original, customized);
assert.deepEqual(rawPngBytes('newMapData/158.png', original), customized);
assert.deepEqual(rawPngBytes('newMapData\\158.png', original), customized);
assert.throws(() => rawPngBytes('newMapData/158.png', Buffer.from('changed source')),
    /source changed/);
const untouched = Buffer.from('unmodified image');
assert.equal(rawPngBytes('newMapData/other.png', untouched), untouched);
console.log('Raw PNG override checks passed.');
