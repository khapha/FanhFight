# FanhFight: sunny pixel woodland

The graphics pass covers the menu, **Woodland**, **Lakeside** and **Rocky Cave**.
The existing player identity, level geometry and collision shapes are preserved.
Roaches use two native pixel frames, turrets have attack warnings, pickups glint,
and water has a translucent shimmering surface. Shared buttons and HUD use a
cream, mint and peach palette. All player-facing labels are English.
Camera boundaries retain their authored 16:9 world extent so narrow windows do
not crop the player at the starting point.

- Backdrop: `Assets/Resources/CuteForest.png`.
- Additional backdrops: `Assets/Resources/CuteLake.png`, `Assets/Resources/CuteCave.png`.
- Background motion and small flowers: `SunnyForest.cs` / `CuteArt.cs`.
- Sliced pixel frames and palette: `CuteTheme.cs`.
- Development screenshots: launch a development build with
  `-cute-preview <absolute-output-directory>` in a visible window. This opt-in smoke test captures
  menu, help, level selection, forest, pause and results, restores the saved run-points
  key, and exits. It does not mark any adventure level complete.

Backdrops are camera children, so Cinemachine cannot move the camera after a
background script has already copied its old position. Player motion and moving
platforms use physics interpolation. Camera lookahead is disabled to avoid
direction-change lurches. Desktop presentation follows display VSync; mobile
targets 60 FPS. Dust and spray share a prewarmed pool capped at 24 emitters, and
score/health labels only rebuild when their values change. Preview runs exercise
all three levels while moving and emitting particles, check camera-local error,
record mean/p95 frame times, and verify that the particle pool drains.

The preview additionally renders both mini-games and restores the existing
high-score and run-points keys. Recorded timings describe the test machine,
not a performance guarantee for other devices.

The three backdrops were generated with the built-in ImageGen tool. UI frames, flowers,
butterflies, moving clouds and terrain textures are native Unity pixel graphics.
The original forest asset pack is retained.

## Validation

- Unity 6000.6.7f1 Windows development build completed successfully.
- 14 standalone regression checks passed, including English UI literals and save compatibility.
- The visible-window smoke test runs at 1280x720 and 540x960, renders all three
  adventure levels and both mini-games, and checks that each backdrop covers the viewport.
- In the three-second movement/effects samples on the test PC, all three levels
  reported mean frame time 16.67 ms and p95 16.68 ms (about 60 FPS with display VSync).
  Camera-local background error was 0.00000 in the sampled frames.
- The eight prewarmed particle emitters were reused and returned after each sample;
  the hard limit remains 24. Duplicate enemy-hit scoring and lethal-damage respawn
  checks passed in the running Unity player.
- Screenshots/logs are under the ignored `TestBuild/` and `Logs/` directories.
  Timings cover short smoke-test segments on this machine, not every combat
  situation or a guarantee for other hardware. Real mobile touch input still
  needs validation on a physical device.

## Additional ImageGen prompts

Both additional backgrounds use `CuteForest.png` as a style reference only.

**Lakeside:** Create a new production background for the Lakeside level of the
same cute pixel-art platformer. Match crisp square pixel clusters, cozy limited
pastel palette, simple stepped silhouettes and friendly atmosphere. Landscape
16:9, side-on. A sparkling turquoise lake, low mint islands and rounded willows,
distant soft blue hills, cream clouds, and tiny pink water lilies near lower
margins. Bright calm morning, spacious pale aqua central sky, layered trees at
the edges, water and reeds in the lower third. No gameplay platforms, characters,
animals, text, UI, frames, logos or watermark. Opaque complete environment.

**Rocky Cave:** Create a new production background for the Rocky Cave level of
the same cute pixel-art platformer. Match crisp square pixel clusters, simple
stepped silhouettes, cozy handheld-game art and controlled pastel palette.
Landscape 16:9, side-on. A welcoming magical cave with lavender/periwinkle rock
arches at the edges, glowing mint and pink crystals in the lower margins, a faint
turquoise underground pool, and mushrooms with cream spots. Spacious low-contrast
lilac center and layered rounded cave walls. Well lit, friendly and dreamy, never
scary or pitch black. Flat color clusters, sharp square pixels, no blur, painterly
brushwork, platforms, characters, enemies, UI, lettering, logos or watermark.

## ImageGen prompt

Use case: stylized-concept. Asset type: production background image for a cute 2D pixel-art platformer game, wide 16:9 landscape composition. Create a charming peaceful sunny woodland with rounded mint-green and sage-green tree canopies, warm honey-brown tree trunks, distant teal rolling hills, cream fluffy clouds, clear pale aqua sky, scattered tiny pastel peach and yellow flowers in the lower margins. Cozy handheld console game art, clearly visible crisp square pixels and stepped silhouettes, limited harmonious pastel palette, flat-color clusters with only two or three shade levels, no smooth gradients, no blur or painted brushstrokes. Side-scrolling platformer background view, not top down or isometric. Distant scenery fills the whole image with a spacious light center for a game menu and characters. Trees frame the left and right sides, middle distance trees small and low contrast. Sky and canopy fill upper half, layered leafy scenery fills lower half; no large foreground objects and no playable platform ground. Friendly, playful, clean and readable. Output a finished background asset without any UI, letters, words, logo, characters, animals, enemies, border or watermark. Opaque background.
