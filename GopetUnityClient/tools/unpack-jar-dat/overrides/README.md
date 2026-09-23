# Raw PNG overrides

`newMapData/158.png` preserves the customized tree committed in `1ab2259`
(2026-09-19). It is an explicit source asset for generation, so rerunning the
unpacker preserves the artwork and `--check` still detects destination drift.

`../overrides.json` pins both the original JAR source and override SHA-256.
Changes to either require reviewing the artwork and updating its hash; a source
change cannot silently keep an obsolete override. Other PNGs remain byte-for-byte
copies of the JAR source.
