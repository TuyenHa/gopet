'use strict';

const assert = require('node:assert/strict');
const { emit, matchesGenerated } = require('./index');

const expected = emit({ 1: 'Tiếng Việt', 2: 'line one\r\nline two' });
assert.equal(matchesGenerated(expected, expected), true);
assert.equal(matchesGenerated(expected.replace(/\n/g, '\r\n'), expected), true);
assert.equal(matchesGenerated(emit({ 1: 'Changed', 2: 'line one\r\nline two' }), expected), false);
assert.equal(matchesGenerated(emit({ 1: 'Tiếng Việt', 2: 'line one\nline two' }), expected), false);
assert.equal(matchesGenerated(emit({ 1: 'Tiếng Việt' }), expected), false);
console.log('String generation checks passed (LF/CRLF, translation drift, escaped newlines, missing keys).');
