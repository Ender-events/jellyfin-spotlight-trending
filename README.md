# Jellyfin TMDB Trending Plugin

[![Build](https://github.com/Ender-events/jellyfin-spotlight-trending/actions/workflows/build.yaml/badge.svg)](https://github.com/Ender-events/jellyfin-spotlight-trending/actions/workflows/build.yaml)

A Jellyfin plugin that displays trending movies and TV shows from TMDB in your Jellyfin instance via the **jellyfin-plugin-media-bar**.

## Features

- ✅ Fetches daily trending content from **The Movie Database (TMDB)**
- ✅ Matches trending items with your **Jellyfin library**
- ✅ Displays up to **15 trending items** in the Media Bar
- ✅ Supports both **movies and TV shows** (interleaved)
- ✅ Automatic daily synchronization (3:00 AM)
- ✅ **Optional** TMDB API key configuration
- ✅ Falls back to Jellyfin's built-in TMDB provider API key

The plugin exposes trending items through the Media Bar plugin and the File Transformation plugin (via `avatars/list.txt`).

## Installation

### Prerequisites

- Jellyfin 10.11.0 or higher (required for API compatibility)
- .NET 9.0 Runtime (usually included with Jellyfin)
- TMDB Account (free at themoviedb.org)
- **jellyfin-plugin-media-bar** (for display)
- **jellyfin-plugin-file-transformation** (exposes `avatars/list.txt` for Media Bar display)

### Install from Repository

**Add Plugin Repository to Jellyfin**

1. Go to: Dashboard → Plugins → Repositories
2. Click **"+"** to add a new repository
3. Enter repository URL: `https://raw.githubusercontent.com/Ender-events/jellyfin-plugins/refs/heads/main/manifest.json`
4. Click **Save**

**Install TMDB Trending Plugin**

1. Go to: Dashboard → Plugins → Catalog
2. Find **"TMDB Trending"** in the list
3. Click **Install**
4. Restart Jellyfin when prompted

**Verify Installation**

1. After restart, go to: Dashboard → Plugins
2. TMDB Trending should appear in your installed plugins list

## Configuration

### TMDB API Key

The plugin requires a TMDB API key. You have two options:

#### Option 1: Use Jellyfin's Built-in TMDB Provider (Recommended)

If you have the **TMDB metadata provider** enabled in Jellyfin, the plugin will automatically use its API key. No additional configuration is needed.

#### Option 2: Configure a Dedicated API Key

1. Get a free API key from [TMDB](https://www.themoviedb.org/settings/api)
2. In Jellyfin, go to **Dashboard > Plugins > TMDB Trending**
3. Enter your TMDB API key
4. Click **Save**

## How It Works

### Data Flow

```
TMDB API (trending/movie/day, trending/tv/day)
         ↓
  [Fetch trending IDs]
         ↓
  [Match with Jellyfin library by TMDB ID]
         ↓
  [Merge with existing list (max 15 items)]
         ↓
  [Save to plugin configuration]
         ↓
  File Transformation Plugin (avatars/list.txt)
         ↓
  Media Bar Plugin
         ↓
  [Display "TMDB Trending" section]
```

### Scheduled Tasks

| Task                                   | Trigger             | Description                                                    |
| -------------------------------------- | ------------------- | -------------------------------------------------------------- |
| `TmdbTrendingSync`                     | Daily at 3:00 AM    | Fetches latest trending content from TMDB and updates the list |
| `Jellyfin.Plugin.TmdbTrending.Startup` | On Jellyfin startup | Registers the Media Bar integration                            |

## Requirements

- **Jellyfin Server** 10.11.0 or later
- **.NET 9.0** runtime
- **jellyfin-plugin-file-transformation** (exposes `avatars/list.txt` for Media Bar display)
- **jellyfin-plugin-media-bar** (for display)
- **Jellyfin TMDB Provider** (optional, for API key fallback)

## Dependencies

- `Jellyfin.Controller` 10.11.\*
- `Jellyfin.Model` 10.11.\*
- `Newtonsoft.Json` 13.\*

## Configuration Reference

| Setting           | Type     | Required | Default | Description                                               |
| ----------------- | -------- | -------- | ------- | --------------------------------------------------------- |
| `TmdbApiKey`      | string   | No       | `""`    | Your TMDB API key. Leave empty to use Jellyfin's provider |
| `TrendingItemIds` | string[] | No       | `[]`    | Internal list of Jellyfin item GUIDs (auto-populated)     |

## Project Structure

```
Jellyfin.Plugin.TmdbTrending/
├── Plugin.cs                          # Plugin entry point
├── PluginServiceRegistrator.cs        # Service registration
├── Configuration/
│   └── PluginConfiguration.cs        # Configuration model
├── ScheduledTasks/
│   ├── TrendingSyncTask.cs           # Main sync task (daily)
│   └── StartupService.cs             # Media Bar registration
├── Services/
│   ├── TmdbService.cs                # TMDB API client
│   └── LibraryMatchService.cs        # Jellyfin library matcher
├── Helpers/
│   └── TrendingListPatch.cs          # Media Bar integration
└── Pages/
    └── configPage.html               # Configuration UI
```

## Building

```bash
# Build in Debug mode
dotnet build Jellyfin.Plugin.TmdbTrending.sln

# Build in Release mode
dotnet publish Jellyfin.Plugin.TmdbTrending -c Release
```

## Acknowledgments

- [Jellyfin](https://jellyfin.org/) - The free media system
- [TMDB](https://www.themoviedb.org/) - The Movie Database API
- [jellyfin-plugin-file-transformation](https://github.com/IAmParadox27/jellyfin-plugin-file-transformation) - Exposes `avatars/list.txt` for Media Bar display
- [jellyfin-plugin-media-bar](https://github.com/IAmParadox27/jellyfin-plugin-media-bar) - Media bar display system
