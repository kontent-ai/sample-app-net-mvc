> [!WARNING]
> This sample app is under active development. Details and documentation will evolve over time.

[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![MIT License][license-shield]][license-url]

# Kontent.ai ASP.NET MVC sample app
<!-- ABOUT THE PROJECT -->
## About The Project

This is a Kontent.ai sample ASP.NET Core MVC application running on **.NET 8**.

It is based on the **Kontent.ai Ficto multisite** project. Once finalized, it will supersede the existing, [legacy .NET sample app](https://github.com/kontent-ai/sample-app-net).

<!-- GETTING STARTED -->
## Getting Started

Follow these steps to get the app running locally.

### Prerequisites

- .NET SDK **8.0** or newer
- A Kontent.ai Ficto multisite sample with:
  - Environment ID
  - (Optional) Preview API key
  - (Optional) Secure Access API key

### Installation

1. Clone this repository:

```bash
git clone https://github.com/kontent-ai/sample-app-net-mvc.git
cd sample-app-net-mvc
```

2. Restore .NET dependencies:

```bash
dotnet restore
```

### Configuration & secrets

1. Set your Kontent.ai environment ID in `appsettings.json`:
   - `DeliveryOptions.EnvironmentId` = your environment ID.

2. Optionally set preview or secure access keys:
> [!CAUTION]
> While for sample app purposes, storing keys directly in `appsettings.json` doesn't present any major risk, even if accidentally committed, consider using local secrets as described below.

```bash
dotnet user-secrets init
dotnet user-secrets set "DeliveryOptions:PreviewApiKey" "your-preview-api-key"
dotnet user-secrets set "DeliveryOptions:SecureAccessApiKey" "your-secure-access-api-key"
```

These values are kept in your local user profile and are not committed to the repository. They will become part of the corresponding `DeliveryOptions` section of `appSettings.json` during runtime.

### Build and run

Build the application:

```bash
dotnet build
```

Run the application:

```bash
dotnet run
```

#### Styles (LESS) in development

To work on styles with automatic recompilation:

```bash
npm install
npm run dev
```

This watches `Styles/site.less` and recompiles to `wwwroot/css/site.css` whenever you save changes.



<!-- USAGE EXAMPLES -->
## Usage

TBD



<!-- CONTRIBUTING -->
## Contributing

For Contributing please see  <a href="./CONTRIBUTING.md">`CONTRIBUTING.md`</a> for more information.



<!-- LICENSE -->
## License

Distributed under the MIT License. See [`LICENSE.md`](./LICENSE.md) for more information.


<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://github.com/kontent-ai/Home/wiki/Checklist-for-publishing-a-new-OS-project#badges-->
[contributors-shield]: https://img.shields.io/github/contributors/kontent-ai/sample-app-net-mvc.svg?style=for-the-badge
[contributors-url]: https://github.com/kontent-ai/sample-app-net-mvc/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/kontent-ai/sample-app-net-mvc.svg?style=for-the-badge
[forks-url]: https://github.com/kontent-ai/sample-app-net-mvc/network/members
[stars-shield]: https://img.shields.io/github/stars/kontent-ai/sample-app-net-mvc.svg?style=for-the-badge
[stars-url]: https://github.com/kontent-ai/sample-app-net-mvc/stargazers
[issues-shield]: https://img.shields.io/github/issues/kontent-ai/sample-app-net-mvc.svg?style=for-the-badge
[issues-url]:https://github.com/kontent-ai/sample-app-net-mvc/issues
[license-shield]: https://img.shields.io/github/license/kontent-ai/sample-app-net-mvc.svg?style=for-the-badge
[license-url]:https://github.com/kontent-ai/sample-app-net-mvc/blob/main/LICENSE.md
[discussion-shield]: https://img.shields.io/discord/821885171984891914?color=%237289DA&label=Kontent%2Eai%20Discord&logo=discord&style=for-the-badge
[discussion-url]: https://discord.com/invite/SKCxwPtevJ
