# Contract: Publish folder and download layout

Where the user guide goes, as built on 2026-10-10 (`Anniversary.Desktop/Anniversary.Desktop.csproj` and
`.github/workflows/ci.yml` at commit `5868c42`). The guide's content: [user-guide-outline.md](user-guide-outline.md);
the system spec: [docs/spec.md](../../../docs/spec.md) §10.

## Publish command

```bash
dotnet publish Anniversary.Desktop -c Release -r <rid> --self-contained \
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish/<rid>
```

RIDs: `win-x64`, `win-arm64`, `osx-arm64`, `osx-x64`, `linux-x64`, `linux-arm64`. CI runs it for each RID on that RID's
own OS; the repository README lists the same commands, written with `publish\<rid>`.

## Folder layout

```text
publish/<rid>/
├── Anniversary              # Anniversary.exe on Windows: the self-contained single-file app
├── README.md                # new: the user guide, a copy of Anniversary.Desktop/README.md
└── THIRD-PARTY-NOTICES.md   # the name-day calendars' license notices, a copy of calendars/THIRD-PARTY-NOTICES.md
```

- The guide is always named `README.md`, the same name as in the project folder.
- Every RID gets the same file: the item has no OS condition.
- The repository's own `README.md`, for developers, is in no project's output.
- At the time, a local macOS build's folder held `Anniversary`, `README.md` and `THIRD-PARTY-NOTICES.md`.

## Project item

In `Anniversary.Desktop/Anniversary.Desktop.csproj`, in the item group with the `THIRD-PARTY-NOTICES.md` item, after
the XML comment "The README for users (spec 10), shipped in every archive and installer." (one line in the file):

```xml
<None Update="README.md"
      CopyToOutputDirectory="PreserveNewest"
      CopyToPublishDirectory="PreserveNewest"
      ExcludeFromSingleFile="true" />
```

| Part | Value | Effect |
|---|---|---|
| `None Update` | `README.md` | sets the metadata of the project folder's `README.md`, which the SDK already includes as a `None` item |
| `CopyToOutputDirectory` | `PreserveNewest` | copied into the build output too, next to the executable |
| `CopyToPublishDirectory` | `PreserveNewest` | copied into the publish folder |
| `ExcludeFromSingleFile` | `true` | kept out of the single-file bundle: a separate file next to the executable |

The `THIRD-PARTY-NOTICES.md` item (`Include="..\calendars\THIRD-PARTY-NOTICES.md" Link="THIRD-PARTY-NOTICES.md"`) has
the same three copy settings.

## CI downloads

`.github/workflows/ci.yml` didn't change: after the tests pass, it publishes each RID and archives the whole folder.

| RIDs | Download | Made with | Contents |
|---|---|---|---|
| `win-x64`, `win-arm64` | `Anniversary-{rid}.zip` | `Compress-Archive -Path publish/{rid}` | one `{rid}` folder with the three files above |
| `osx-arm64`, `osx-x64`, `linux-x64`, `linux-arm64` | `Anniversary-{rid}.tar.gz` | `tar -czf … -C publish {rid}` (keeps the executable bit) | the same |

Each archive is attached to the run as it is (not zipped again) and kept for 30 days; downloading needs a GitHub
sign-in.

## Later

- [010](../../010-install-packages/spec.md) builds a package from the same folder: the Windows installer installs
  `README.md` and `THIRD-PARTY-NOTICES.md` next to `Anniversary.exe`, the `.dmg` holds them next to `Anniversary.app`
  (and the bundle has them inside), and the AppImage keeps them in `usr/share/doc/anniversary` (docs/spec.md §10).
