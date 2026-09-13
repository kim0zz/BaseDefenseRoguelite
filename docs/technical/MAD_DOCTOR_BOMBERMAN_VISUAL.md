# Bomberman — Mad Doctor visual prototype

Status: visual prototype for playtest, 2026-09-12. No balance or FROZEN design changes.

## Asset

- Source: `ArtSource/MadDoctor/MadDoctor.blend` (Blender 5.2, packed palette).
- Runtime: `Assets/_Game/Art/MadDoctor/MadDoctor_Visual.prefab`.
- 1448 triangles including goggles, hair, gloves, boots, hip bombs and reactor.
- One skinned mesh, one URP Lit palette material, 12 bones. No visual colliders.
- Eight FBX clips: Idle, Run, Throw, Place, Detonate, Kick, Cast, Death.
- Feet at local Y=0 in Unity; forward +Z; unscaled height about 2.34 m.
- FBX primary UV channel is PaletteUV. Keep only this channel when exporting; primitive UV maps otherwise produce incorrect colors in Unity.
- Source lives outside Assets so Unity does not need Blender to import or build the project.

## Integration

`CombatBootstrap.bombermanVisualPrefab` adds the visual only for the Bomberman class. BootScene uses the existing P1 Bomberman playtest override. Other classes and enemy visuals retain their previous behavior.

`BombermanModelView` hides the capsule renderer and old hand-held weapon placeholder, while retaining the existing aim arc and all gameplay components. The player convention currently removes the capsule collider at spawn; the model adds none. The visual has the existing player-relative feet offset of -1 m.

Idle/Run use the locomotion layer. Throw, Detonate and Cast affect the upper body, preserving running legs. Place, Kick and Death use a full-body layer. Animation contact at normalized time .5 is mapped to the actual attack/skill windup end. Read-only cycle elapsed properties keep timing consistent with attack-speed changes. No animation events apply damage or move the gameplay root.

Damage flashes the skinned renderer. Death plays a tinted falling pose; normal PlayerRespawn restores living colors and locomotion after 20 seconds. An active slot-3 ultimate uses Cast; the prototype does not unlock ultimates early. Existing projectile visuals, explosions and skill audio remain in use.

## Verification

Manual integration harness: `Assets/_Game/Tests/Manual/BombermanVisualPlaytest.cs`, excluded from player builds with UNITY_EDITOR. Invoke only in an empty Play Mode test scene with a CombatBootstrap configured with the prefab and Bomberman override. Call `Begin(outputDirectory)` on the harness; it creates a synthetic gamepad and test players. Do not run it in a live run. Stop Play afterwards.

Checks cover model assignment, triangle/material budget, movement and actual bone rotation, thrown projectile creation, Q bomb placement, E detonation, R bomb kick, skill animation/cooldown, active ultimate animation, damage flash, death/root-motion isolation, normal respawn and another class retaining its capsule.

Result: 21/21 integration checks passed; see `ArtSource/MadDoctor/playtest-results.txt`. The final smoke run had no Unity Console errors or warnings. A subsequent small visibility correction hides all pending/rebuilt weapon-placeholder children immediately at initialization as well as each frame.

Windows Development build was attempted and FAILED in UnityLinker: `Failed to resolve assembly: nunit.framework, Version=3.5.0.0`. Existing NUnit files under `Assets/_Game/Tests/EditMode` compile in the default runtime assembly (no Editor folder/test assembly boundary). This is outside the model change; a standalone executable is not verified. The build also reported stripping of the existing Hidden/Core debug occlusion shaders. Editor Play Mode works; do not describe the standalone build as passing.

Rendered Blender portrait and Unity captures verify palette and silhouette. This is a basic rigid-weight low-poly rig; full-body Place/Kick can show foot sliding while movement is allowed by gameplay. Four-player readability, long-run performance and final game feel still require a human playtest. No milestone acceptance is implied.

The two Python scripts in ArtSource are reconstruction scripts for Blender, run in order in a fresh file. Adjust their OUT and ART paths on another machine; the packed .blend is the primary editable source.
