# Changelog

All notable changes to this package will be documented in this file.

## [1.0.0] - 2026-08-25
### Added
- Standardized package structure for WASD Mobile Studio and UPM.
- Integrated `AdjustTrackingProvider` implementing `WASD.Core.Services.Tracking.ITrackingProvider` & `WASD.Core.Services.Analytics.IAnalyticsProvider`.
- Added Impression-Level Ad Revenue logging support via `Adjust.TrackAdRevenue`.
- Added `AdjustEditorWindow` for configuration management.
- Included official Adjust Unity SDK v5.4.0 with deep linking, SKAdNetwork & IDFA support.
- Added `link.xml` for IL2CPP stripping safety.
