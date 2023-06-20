
<p align="center">
<h1>
<bold>🚧 This project is still in the planning phase. 🚧</bold>
</h1>
</p>

# Confix - Simple Application Configuration

<p align="center">
<img src="images/logo.png">
<p>

- [Confix - Simple Application Configuration](#confix---simple-application-configuration)
  - [Introduction](#introduction)
  - [Principles](#principles)
    - [Simplicity](#simplicity)
    - [Composability](#composability)
    - [Flexibility](#flexibility)
    - [Security](#security)
- [Components](#components)
  - [File Structure](#file-structure)
  - [Package Distribution](#package-distribution)
  - [Component Inputs and Outputs](#component-inputs-and-outputs)
  - [Commands](#commands)
  - [Pipeline Instead of Separate Inputs and Outputs](#pipeline-instead-of-separate-inputs-and-outputs)
- [Variables](#variables)
  - [Providers](#providers)
  - [Example JSON with Variables](#example-json-with-variables)
  - [Commands](#commands-1)
- [Project](#project)
  - [Overview](#overview)
  - [Components](#components-1)
  - [Override Configuration](#override-configuration)
    - [Environments](#environments)
    - [Component Providers \& Repositories](#component-providers--repositories)
  - [Configuration Files](#configuration-files)
  - [Commands](#commands-2)
- [Repositories](#repositories)
  - [Setting Defaults](#setting-defaults)
  - [File Structure](#file-structure-1)
    - [Repository Root](#repository-root)
    - [Monorepo](#monorepo)
- [Distributing Components](#distributing-components)
  - [Component Discovery](#component-discovery)
  - [Component Repositories](#component-repositories)
- [Environments](#environments-1)
- [Variable Providers](#variable-providers)
- [Subprojects](#subprojects)
- [Configuration Files](#configuration-files-1)
- [`.confix` Files](#confix-files)
  - [1. `.confixrc`](#1-confixrc)
  - [2. `.confix.repository`](#2-confixrepository)
  - [3. `.confix.project`](#3-confixproject)
  - [4. `.confix.component`](#4-confixcomponent)
  - [5. `.confix.lock`](#5-confixlock)
- [Deploying your App](#deploying-your-app)
  - [Build Time](#build-time)
  - [Deploy Time](#deploy-time)
    - [Procedure](#procedure)
  - [Runtime](#runtime)
 




## Introduction

> **ℹ️ This section is still in development. You can leave your feedback in the [Discussions](https://github.com/SwissLife-OSS/Confix/discussions/2) about this section.**

Managing application configuration has evolved into an intricate process in the modern software development landscape. Enter Confix, a tool specifically designed to untangle this complexity. Confix aids you in managing your application configuration throughout different stages of the software lifecycle, spanning from local development to production deployment. 

Everything you need is the `confix cli` and `VSCode`

<p align="center">
<img src="images/intro-0.png">
<p>

## Principles

The principles guiding the design and development of Confix include Simplicity, Composability, Flexibility, and Security.

### Simplicity

Building an application locally is relatively straightforward, yet deploying the same application often poses significant challenges. In theory, deploying your app to a cloud provider seems like a breeze, but the reality often contrasts with this expectation. On a local machine, running an application is as easy as executing `dotnet run` or `npm start`, utilizing a local database, without having to worry about secrets or similar factors.

However, when it comes to deployment, you might find yourself grappling with concerns like, "How do I get this configuration into my app?" Confix simplifies this process; you merely store the configuration in a JSON file in your Git repository. Through the JSON Schemas, your IDE will help you with writing the configuration correctly with code completion and validation.

### Composability

In enterprise settings, you frequently encounter shared code. There might be an internal library that you wish to reuse in multiple locations or across various services. You might, for example, only want to create extensions for configuring logging or the database connection once. 

To facilitate this, Confix encourages reusability of configuration. With Confix, you can compose components that help you reuse parts of your shared code across your applications and services.

### Flexibility

Confix is built upon the philosophy that every configuration file should be a JSON file that is easy to create and consume. If your application can load JSON files, it can utilize Confix as an app configuration manager. This principle ensures that you can load your configuration the same way, regardless of whether you are in a production environment or in a development one.

### Security

While Confix is a powerful tool for managing application configuration, it is not designed to manage secrets. Instead, it provides a mechanism for representing secrets within your configuration using variables, but it does not store the secrets themselves. The actual secret storage is left to your discretion and your preferred storage method.

With Confix, you can easily integrate your preferred method of storing secrets. You might choose to store your secrets in Azure Key Vault, HashiCorp Vault, or perhaps as environment variables. Alternatively, you may decide to encrypt them and store them in the same JSON file as your other configuration data.

By adhering to these principles, Confix aims to streamline the application configuration process, making it more efficient and less error-prone. Enjoy the simplicity and flexibility of Confix, and say goodbye to the complications of application configuration.

# Components

> **ℹ️ This section is still in development. You can leave your feedback in the [Discussions](https://github.com/SwissLife-OSS/Confix/discussions/3) about this section.**

In Confix, "components" are a key construct designed to streamline shared code usage. A typical use case involves shared packages that contain reusable code, such as a logging module that's frequently used across multiple applications or (micro)services.

Each component in Confix defines its own schema. This schema acts as a blueprint for configuration validation, ensuring that the configuration matches the predefined structure.

The schema of a component is a JSON schema. Don't worry about having to write this yourself - Confix is designed to simplify schema creation for developers. Confix provides a variety of schema compilers that can generate a schema based on your code. For example, the GraphQL compiler generates a schema based on a GraphQL file, while the .NET annotations compiler produces a schema derived from the configuration.

Creating a new component in Confix is as simple as running the command `confix component create <componentName>`. This command generates a new `.confix.component` file which is then stored in `./components/<componentName>`. This file contains the component's configuration, including the component's name, inputs, and outputs. 

e.g. 
```json
{
  "name": "Logging",
  "inputs": [
    {
      "type": "graphql"
      // additional properties specific to the graphql compiler
    }
  ],
  "outputs": [
    {
      "type": "dotnet-options-classes",
        // additional properties specific to the json output
    }
  ]
}
```

> The `inputs` and `outputs` properties are optional. If you do not specify any inputs or outputs, Confix will use the default inputs that are defined in the `.confixrc` or the `.confix.project` file.


## File Structure 
All components are saved in the `Components` folder, situated at the root of your project. Each `.confix.component` file is a JSON file that encapsulates the details of the component's configuration. This file must contain a 'name' property specifying the component's name.

```
myProject
├── Components
│   ├── Logging
│   │   ├── .confix.component
│   │   ├── schema.json
│   │   └── schema.graphql
│   └── DistributedCache
│       ├── .confix.component
│       ├── schema.json
│       └── schema.graphql
├── ...
```

## Package Distribution
Confix has different methods for distributing packages. The most common method is to bundle the schema with the package itself. When you distribute your package, whether through NuGet, NPM, or Azure DevOps, the corresponding schema is included within the package itself. This practice automatically versions your component schema together with your code. For example, when you reference version 1.20.4 of your package, you also obtain the matching version of the schema.

Additional distribution methods will be elaborated upon in the 'Distributing Components' section of this documentation.
 

## Component Inputs and Outputs

A Confix component can have one or multiple 'inputs'. These inputs function as schema compilers, which create new `schema.json` files or update existing ones based on the input provided. Inputs are executed in the sequence they are defined in the `.confix.component` file.

Examples of inputs are the GraphQL compiler, which generates a JSON schema based on a GraphQL file, and the .NET annotations compiler, which produces a schema derived from the configuration. 

In addition to inputs, a component can also have one or multiple outputs. These outputs are generated from the finalized `schema.json` file. By default, Confix searches for a `.confix.component` file in the shipped bundle to locate the `schema.json` file, which is then used as the JSON schema for configuration.

## Commands

Several commands are available for managing components in Confix:

- To create a new component, use: `confix component create <componentName>`
- To build a component, use: `confix component build <componentName>`
- To list all existing components, use: `confix component list`
- To delete a component, use: `confix component delete <componentName>`

These commands provide a user-friendly way to manage and interact with your Confix components effectively.

<details>
<summary>🚧 Open Questions</summary>

## Pipeline Instead of Separate Inputs and Outputs

Currently, Confix uses a two-stage process with inputs processed first, followed by outputs. This structure behaves somewhat like a pipeline, sequentially processing tasks in a predefined order. To enhance flexibility, we could reconfigure this into a single 'pipeline' property that determines the execution sequence.

While a single pipeline could increase configuration complexity, it offers more control over the transformation and processing of schemas.

For instance, after a GraphQL schema has been compiled, you might want to cleanse it and then generate a new schema based on the cleansed version. A pipeline setup would make such processes straightforward. 

Here's an illustrative example of what a pipeline configuration might look like:

```json
{
  "name": "Logging",
  "pipeline": [
    {
      "type": "graphql-compiler",  // Initial schema generation from GraphQL
    },
    {
      "type": "dotnet-generator",  // Generate C# classes from the schema
    },
    {
      "type": "schema-cleaner",  // Clean the schema removing unnecessary details
    },
    {
      "type": "dotnet-schema-compiler",  // Compile a new schema.json based on the cleaned schema
    },
    {
      "type": "dotnet-config-doc-generator"  // Generate documentation based on the final schema
    }
  ]
}
```
In this example, each step of the pipeline is defined by a `type`. These types specify what operation to perform on the schema or the resulting data, moving through compiling, cleaning, and finally, documentation generation.

Despite the benefits of a single pipeline structure, one trade-off to consider is the increase in complexity when adjusting the configuration. For instance, if you only want to modify a project's output - say, to generate additional JavaScript code - you'd need to override the entire pipeline instead of simply adding a new output. This requirement might add an extra layer of complexity to managing your project's configuration. Therefore, it's essential to weigh the flexibility benefits against potential configuration challenges when considering a pipeline approach.
</details>

# Variables
> **ℹ️ This section is still in development. You can leave your feedback in the [Discussions](https://github.com/SwissLife-OSS/Confix/discussions/4) about this section.*

Confix has the concept of variables. These variables are typically used to reference secrets, or any configuration data that should not be directly entered into the configuration files. They can be used for values are not known at development time. 

In Confix, variables have a distinct structure: `$providerName:path.to.resource`, where `providerName` refers to the source or provider of the variable value, and `path.to.resource` indicates the specific resource or secret in the provider's repository.

Variables are resolved during the `build` operation of the configuration. This operation can take place at various stages of your development lifecycle. You can also `validate` your configuration against a specific environment to ensure that all variables can be resolved before deployment. For more on this, see the `Deploying your App` section.

The specification for variable providers is contained in the `.confix.project` file, or inherited from `.confixrc`. During project initialization, all potential variables are fetched from the providers (if possible), offering intelligent code completion during your development.

## Providers

Different providers can be utilized to manage your variables:

- **$keyvault**: Use this when your variables are stored in Azure Key Vault. The path should be the name of the variable as it is in the key vault.
  
- **$vault**: For those utilizing HashiCorp Vault for secret management, this is the appropriate provider.
  
- **\$secret**: This provider allows you to inline the secrets directly into your config using public-private key encryption. For instance, `$secret:aHR0cHM6Ly93d3cueW91dHViZS5jb20vd2F0Y2g/dj1hM1o3ekVjN0FYUQ==`.

- **$local**: Use this provider to reference local variables, typically stored in a `variables.json` file in your document root.
  
- **$git**: This provider fetches variables from a specific git repository.

## Example JSON with Variables

Here's an example JSON configuration file that uses Confix variables:

```json
{
  "database": {
    "connectionString": "$keyvault:database.connectionString",
    "maxPoolSize": 12 
  },
  "logging": {
    "level": "$git:logging.level",
    "destinatin": "$git:logging.server.url"
  },
  "appSecrets": {
    "apiKey": "$secret:aHR0cHM6Ly93d3cueW91dHViZS5jb20vd2F0Y2g/dj1hM1o3ekVjN0FYUQ==",
    "jwtSecret": "$vault:jwt.secret"
  }
}
```

## Commands

To manage variables in Confix, you can use the following commands:

- `confix variables reload`: This command reloads the variables for a project from the providers. Useful for updating your local environment with newly created variables.

- `confix variables set <variable> <value>`: This command sets the value for a specified variable.

- `confix variables get <variable>`: This command retrieves the current value of a specified variable.

# Project
>  **ℹ️ This section is still in development. You can leave your feedback in the [Discussions](https://github.com/SwissLife-OSS/Confix/discussions/5) about this section.**

## Overview

A Confix project is essentially a folder that houses your configuration consumer. Within this folder, you will find configuration files that Confix utilizes to manage them. Confix auto-discovers the required structure of your configuration file by collecting all relevant components, and then generates a JSON schema that corresponds to your configuration.

At the heart of the Confix project is the `.confix.project` file, which provides the necessary configuration for your project.

## Components
You can adjust the configuration of components at the project level using the `components` property. 

Components found by the component providers are automatically incorporated into the project and don't need explicit declaration under the `components` property. However, if you want to modify a component's configuration, you can do so using this property.

In contrast, components from repositories aren't automatically added to the project. You need to define these explicitly in the `components` property.

The `components` property is an object where the key is either `@providerName/componentName` or `@repositoryName/componentName`. The value associated with each key can take several forms:

- A string specifying the version of the component
- A boolean indicating if the component should be included in the project
- An object for more detailed component configuration, which can contain:
  - `version`: Specifies the version of the component (only relevant for components from repositories)
  - `include`: A boolean indicating if the component should be included in the project (useful for overriding discovery providers)
  - `mountingPoint`: Specifies a JSON path or a list of JSON paths defining where in the configuration the component should be inserted. If this property is not specified, the component will be added at the root of the configuration under the component's name.

## Override Configuration
### Environments 
By default, the environment config is taken from the `.confixrc` or the `.confix.repository`. 
To override environments, on a project level, you'll need to use the `environments` property in the `.confix.project` file. This property is an array of strings, with each string representing an environment. 

The environment that you specify in this list, will be enabled for this project. If you leave the property away, all environments are active.

Here's how it works:

```json
{
  "environments": ["development", "staging", "production"]
}
```
### Component Providers & Repositories  
You can override the component providers and repositories on a project level. This is useful if you want to use different providers or repositories for a specific project.

See the `Component Providers` and `Component Repositories` sections for more information.

## Configuration Files 

Your Confix project can have multiple `configurationFiles` as explained in the `Configuration Files` section. You can override these files on a project level. This is useful if you want to use different configuration files for a specific project. 

Read more about this in the `Configuration Files` section.

## Commands 

Confix provides several commands to manage your project:

- `confix project init`: Initializes a new project
- `confix project build`: builds the final configuration files of the project
- `confix project reload`: Reloads the configuration of the component providers 
- `confix project validate`: Validates the configuration of the current project

Here is an example of a `.confix.project` file:

> Note: Unless you use component repositories everything in this file is optiona. If you do not specify any environments, configuration files, configuration providers or repositories, Confix will use the defaultx that are defined in the `.confixrc` or in `.confix.repository` file. So dont worry your `.confix.project` **file will most probably be empty**, as you configure everything on the .confixrc level or in the .confix.repository files.

```json
{
    "name": "Some-Project", // (Optional) this is by default inferred from the folder 
    "environments": [ "dev", "prod" ], // (Optional)
    "components": {
        "@charts/MyHelmComponent": "latest",
        "@charts/someOtherComponent": "latest",
        "@common-components/CloudComponent": false,
        "@dotnet-package/BlobStorage": {
            "mountingPoint": [
                "documents/blob-storage",
                "user-data/blob-storage"
            ]
        },
        "@oss-components/CustomComponent": "1.0.0"
    },
    "configurationFiles": [ // Optional when in .confixrc or .confix.repository
        "./appsettings*.json",
        "./**/some-config/appsettings*.json",
        {
            "type": "dotnet-appsettings"
        }
    ],
}
```

# Repositories
>  **ℹ️ This section is still in development. You can leave your feedback in the [Discussions](https://github.com/SwissLife-OSS/Confix/discussions/6) about this section.**

Confix repositories closely align with the "scope" of a Git repository. Meaning that usually you have one repository per git repository. In a typical repository, you might find various projects, modules, or packages. These can be backend services, workers, shared libraries, frontend applications, etc., each with could have separate configuration.

Every project, module, or package that has configuration files, should contain a `.confix.project` file at its root, which serves as the project's main configuration. The repository's root folder should contain a `.confix.repository` file, providing an overarching configuration for the entire repository.

Confix generates a configuration for Visual Studio Code in the `.vscode` folder at the repository level. This means that when you need to configure your services' config files, you have open the repository root in Visual Studio Code. Otherwise you do not have the intellisense and validation (as the correct json schema is not loaded)

For larger structures, such as mono-repos, you may find multiple `.confix.repository` files. In this case, the repository should be opened in Visual Studio Code from the folder containing the relevant `.confix.repository` file.

To share the setup across repositories in a monorepo, you can create a `.confixrc` file at the root. When doing so, remember to set `"isRoot": false` in the configuration, prompting Confix to continue looking for the root `.confixrc` file. 

## Setting Defaults 

In repositories, you can predefine defaults for your `.confix.project` files using the `project` property. This feature promotes consistency and reduces redundancy by ensuring standard configurations across different projects.

Similarly, if you wish to establish default configurations for all components, use the `component` property. This ability streamlines the component configuration process and ensures uniformity across your entire repository.

You can also override or specify the variable providers and environments on a repository level. This is useful if you want to use different providers or repositories for a specific repository. 


```json
{
    "environments": [ "dev", "prod" ], // Optional when in .confixrc
    "variableProviders" :[
        // variableProviders config . Optional when in .confixrc
    ],
    "project": {
        // config like in .confix.project
    }, 
    "component": {
        // config like in .confix.component
    },
    "v
}
```

## File Structure
### Repository Root
```
repo_root/
├── .confix.repository
├── .vscode
├── backend
│   ├── .confix.project
│   └── appsettings.json
├── worker
│   ├── .confix.project
│   └── appsettings.json
├── shared_lib
└── frontend
```
### Monorepo
```
monorepo_root/
├── .confixrc
├── repo_1/
│   ├── .confix.repository
│   ├── .vscode
│   ├── backend
│   │   ├── .confix.project
│   │   └── appsettings.json
│   └── worker
│       ├── .confix.project
│       └── appsettings.json
└── repo_2/
    ├── .confix.repository
    ├── .vscode
    ├── shared_lib
    └── frontend
        ├── .confix.project
        └── appsettings.json
```



# Distributing Components
>  **ℹ️ This section is still in development. You can leave your feedback in the [Discussions](https://github.com/SwissLife-OSS/Confix/discussions/7) about this section.**

In Confix, we have two primary methods to distribute components: Direct Shipping and Repositories.

1. **Discovery:** This is the preferred method for component distribution. Here, you bundle the components directly with your package. This approach facilitates seamless integration of the components with your project, eliminating the need for additional retrieval or loading operations.

2. **Repositories:** Alternatively, you can utilize repositories to load components from various storage providers. This method offers flexibility and can be particularly useful in scenarios where the components are stored externally or need to be versioned independently of the primary project.

For efficient component management, we recommend configuring the component discovery and repository settings globally in the `.confixrc` file or locally in the `confix.repository` file for repository-specific configurations. While it's possible to make project-specific adjustments in the `.confix.project` file, we suggest this approach only when absolutely necessary. Sharing configurations across projects helps maintain consistency, reduces duplication, and allows for easier maintenance.

## Component Discovery 

You can specify how your projects discover the components used in your configuration by defining the `componentProviders`. Component providers are responsible for loading the components used by your project whenever you 'reload' the project. As you bundle the code/binaries of the component together with the component on build time, you guarantee that you configuration always matches the used version of the component.

There are several types of component providers available, such as `dotnet-package`, which scans the project's assemblies for embedded resources describing a Confix component, and `node-modules`, which checks the node-modules used by the project for Confix components.


```json
{
    "componentProviders": [
        {
            "name": "dotnet",
            "type": "dotnet-package"
        },
        {
            "name": "modules",
            "type": "node-modules"
        }
    ],
}
```

## Component Repositories 

Component repositories come into play when you're unable to bundle a schema with your code or when components need separate versioning (for instance, in build pipeline configuration). These repositories may exist in various locations such as Azure Blob storage (`blob`), Helm repositories (`helm`), Git repositories (`git`), or public JSON schemas (`json-schema`).

```json
{
    "componentRepositories": [
        {
            "name": "common-components",
            "type": "blob",
            "url": "https://myblobstorage.blobl.core.windows.net/confix"
        },
        {
            "name": "charts",
            "type": "helm",
            "url": "oci://registry/namespace/chart-name"
        },
        {
            "name": "oss-components",
            "type": "git",
            "url": "git@github.com:SwissLife-OSS/template.git"
        },
        {
            "name": "travis",
            "type": "json-schema",
            "url": "https://json.schemastore.org/travis.json"
        }
    ]
}
```

# Environments
>  **ℹ️ This section is still in development. You can leave your feedback in the [Discussions](https://github.com/SwissLife-OSS/Confix/discussions/8) about this section.***

It is common practice to deploy applications across various environments such as development, staging, or production. Each environment may require a unique set of configurations and variable providers. To accommodate this, Confix enables you to define the environments where your application will be deployed.

The `environments` field allows you to define the various environments for your application in `.confixrc`, `.confix.repository`, or `.confix.project` files. These environments are inherited in the order: `.confixrc` -> `.confix.repository` -> `.confix.project`. Hence, settings defined in `.confixrc` can be overridden in `.confix.repository`, and in turn, these can be overridden in `.confix.project`.

In certain scenarios, you might want to selectively enable or disable environments at a subfolder level. For example, if you wish to disable the 'staging' environment in a specific subfolder, you can do so by simply listing the active environments like `["dev", "prod"]` in the `environments` field for that specific configuration level. This will exclude 'staging' from the active environments for the respective configuration scope.

Here are the properties that can be configured for each environment object:

- `name`: This property sets the name of the environment, serving as a unique identifier.

- `excludeFiles`: This property lists the configuration files that should be excluded from this particular environment. This can be useful when certain variables are only applicable to specific environments. For example, if a variable is present in 'staging' but not in 'production', you could use this property to exclude the configuration file containing that variable when in the 'production' environment. This prevents validation issues due to non-existent variables in the configuration.

- `includeFiles`: In contrast to `excludeFiles`, this property identifies the configuration files that should be included in the environment. Any configuration files not listed here are excluded from the environment.

- `enabled`: This property indicates whether the environment is active or not. 

With these advanced configuration options, Confix provides you with a high level of control, allowing you to customize how each environment handles configuration files and variables.

Here's an example of how you could define these settings in a `.confix.project` JSON file:

```json
{
  "environments": [
    {
      "name": "development",
      "excludeFiles": ["appsettings.staging.json"]
    },
    "staging",
    {
      "name": "production",
      "excludeFiles": ["appsettings.staging.json"]
    }
  ]
}
```

# Variable Providers
>  ℹ️ This section is still in development. You can leave your feedback in the [Discussions](https://github.com/SwissLife-OSS/Confix/discussions/9) about this section.

Variable providers are typically defined in the `.confixrc` or `.confix.repository` files but can also be defined in `.confix.project` if only have a simple setup without a `.confix.repository`

The `variableProviders` property determines the variable providers that are available for your project. These providers assist in resolving variables present in your configuration files.

Each variable provider is identified by a name, which is how it's referenced in your config files. For instance, if you have a variable provider defined as `"name": "secret"`, it can be referenced in your configuration files using `$secret`.

The `type` field defines the type of the provider. Each type carries its specific configuration. For example, the `azure-keyvault` type would require configuration related to Azure Keyvault, while the `secret` type would need details about public and private keys.

The `environmentOverride` attribute lets you override provider-specific configuration based on the environment. This can be useful when, for instance, a provider has different URLs for different environments like development, staging, or production.

Here is an example JSON demonstrating the structure of the `variableProviders` property:

```json
{
    "variableProviders": [
        {
            "name": "keyvault",
            "type": "azure-keyvault",
            "environmentOverride": {
                "dev": {
                    "url": "https://mykeyvault-dev.vault.azure.net"
                },
                "qa": {
                    "url": "https://mykeyvault-qa.vault.azure.net"
                },
                "prod": {
                    "url": "https://mykeyvault-prod.vault.azure.net"
                }
            }
        },
        {
            "name": "vault",
            "type": "hashicorp-vault",
            "environmentOverride": {
                "dev": {
                    "url": "https://myvault-dev.vault.azure.net"
                },
                "qa": {
                    "url": "https://myvault-qa.vault.azure.net"
                },
                "prod": {
                    "url": "https://myvault-prod.vault.azure.net"
                }
            }
        },
        {
            "name": "secret",
            "type": "secret",
            "environmentOverride": {
                "dev": {
                    "publicKey": "./certs/dev/public.pem",
                    "privateKey": "./certs/dev/private.pem"
                },
                "qa": {
                    "publicKey": "./certs/qa/public.pem"
                },
                "prod": {
                    "publicKey": "./certs/prod/public.pem"
                }
            }
        },
        {
            "name": "local",
            "type": "local",
            "path": "./variables.json"
        }
    ]
}
```

In this example, the `keyvault`, `vault`, `secret`, and `local` variable providers are defined, each with their type and environment-specific overrides where applicable.

# Subprojects 
>  ℹ️ This section is still in development. You can leave your feedback in the [Discussions](https://github.com/SwissLife-OSS/Confix/discussions/10) about this section.

A project can contain different active schemas for various configuration files. For instance, you may have a .NET application using `appsettings.json` and a Helm chart for configuring Kubernetes. For such cases, you can specify additional projects using the `subprojects` property.

Subprojects define unique project configurations, including their custom components, repositories, and component providers. By default, a subproject inherits the configuration from the parent project, hence any unwanted inherited features should be specifically disabled. It's essential to always specify the complete configuration, with the `name` property being a mandatory requirement.

Moreover, subprojects can be configured in the `.confixrc` or `.confix.repository` files, allowing global definition of subprojects that are then inherited into the `.confix.project` file.

One practical application of this feature could be in scenarios where a common configuration pattern for Helm charts exists. A subproject can be defined for this purpose, and then applied across all your projects.

```
root_project
├── .confix.project
├── .docker
│   ├── dev
│   │   └── config.yml
│   ├── staging
│   │   └── config.yml
│   └── production
│       └── config.yml
└── src
    └── Host
        ├── Program.cs
        ...
```

For instance, if you have a .docker folder containing the folders dev, staging, and production, with each of these folders containing a config.yml file defining the Kubernetes deployment chart of a particular service, you can conveniently configure this to function across all your projects, as demonstrated below:

```json
{
    "subprojects": [
        {
            "name": "helm",
            "components": {
                "@charts/SharedConfig": "latest",
            },
            "configurationFiles": [
                "./helm/**/values*.yaml"
            ]
        }
    ]
}
```

All your services now can use the same configuration for the Kubernetes deployment chart, with the `SharedConfig` component being included in the `values.yaml` file and be automatically updated whenever you run `confix project build`.

# Configuration Files 
>  ℹ️ This section is still in development. You can leave your feedback in the [Discussions](https://github.com/SwissLife-OSS/Confix/discussions/11) about this section.

Confix allows you to specify the configuration files used by your project setup using the `configurationFiles` property. Essentially, it lets you identify which JSON files in your project should be validated against the compiled schema.

One key feature of Confix is its capability to automatically modify the `settings.json` of Visual Studio Code in the repository root, mapping a JSON schema to the specified files. For instance, in ASP.NET core projects, these configuration files would typically be the `appsettings.json` files. In Node.js projects, it might be the `config.json` file.

Confix simplifies the configuration process by providing several configuration file conventions. You can define these conventions in the `configurationFiles` property of the `.confixrc`, `.confix.repository`, or `.confix.project` files.

Below is an example of defining these settings in a `.confix.project` JSON file:

```json
{
  "configurationFiles": [
    "./appsettings*.json",
    "./**/some-config/appsettings*.json",
    {
      "type": "dotnet-appsettings"
    }
  ]
}
```

The `configurationFiles` property can be expressed in several ways:

- A string specifying the path to the configuration file (or a glob pattern)
- An object describing the configuration file convention
- An array containing a combination of the above two options

When you specify a string or a glob pattern, Confix will infer the configuration file convention based on the file extension. For instance, if you provide the path `./appsettings*.json`, Confix will automatically identify these files as JSON configuration files.

Alternatively, you can explicitly define the configuration file convention using an object. For example, by specifying the "dotnet-appsettings" convention, Confix will scan your project for any `appsettings.json` files and generate a schema for them if found.

```json
{
    "type": "dotnet-appsettings"
}
```

Generally, configuration file globs are defined at the repository level, since they are usually consistent across all projects in the repository. You can override these settings at the project level if necessary. However, if you want to define conventions (like "dotnet-appsettings"), you should ideally specify them in the `.confixrc` file.

# `.confix` Files 
>  ℹ️ This section is still in development. You can leave your feedback in the [Discussions](https://github.com/SwissLife-OSS/Confix/discussions/12) about this section.

Confix provides a set of configuration files that allow you to manage settings on global, repository, project, component, and deployment levels. These files include: `.confixrc`, `.confix.repository`, `.confix.project`, `.confix.component`, and `.confix.lock`. 

Each configuration file serves a unique purpose in the overall Confix configuration process:

## 1. `.confixrc` 

This global configuration file is located in the user's home directory. It is used to define global settings that are applied across all repositories and projects. Settings defined here are inherited by the `.confix.repository`, `.confix.component`, and `.confix.project` files. 

You can override project-specific settings by using the `project` field in the `.confixrc` file. In case of multiple `.confixrc` files, settings can be overridden in the parent folder's `.confixrc` files. To signal Confix to look for further `.confixrc` files in parent folders and the home directory, you need to specify `isRoot: false`.
```json
{
    "isRoot": false,
    "project": {
        // default project config like in .confix.project,
        "subprojects": [
            // default subprojects config check the subprojects section
        ]
    },
    "component": {
        // default component config like in .confix.component
    }
}
```

## 2. `.confix.repository`

This configuration file is defined at the root of the repository. It is used to specify repository-specific settings applicable across all projects within the repository. Settings defined here are inherited by the `.confix.component` and `.confix.project` files. 

Project-specific settings can be overridden by using the `project` field in the `.confix.repository` file. For more information, refer to the `Repositories` section.
```json
{
    "project": {
        // default project config like in .confix.project,
        "subprojects": [
            // default subprojects config check the subprojects section
        ]
    },
    "component": {
        // default component config like in .confix.component
    }
}
```

## 3. `.confix.project`

This file is used to configure a specific project. You can find more detailed information about this in the `Project` section.

```json
{
    "name": "MyProject",
    "environments": [
        // environments config check the environments section
    ],
    "components": {
        // components config check the components section
    },
    "configurationFiles": [
        // configurationFiles config check the configuration files section
    ],
    "subprojects": [
        // subprojects config check the subprojects section
    ]
}
```
## 4. `.confix.component`

The `.confix.component` file is used for configuring individual components. More about this can be found in the `Components` section.

```json
{
    "name": "MyComponent",
    "inputs": [
        {
        "type": "graphql"
        // additional properties specific to the graphql compiler
        }
    ],
    "outputs": [
        {
        "type": "dotnet-options-classes",
            // additional properties specific to the json output
        }
    ]
}
```
## 5. `.confix.lock`

The `.confix.lock` file is generated during the `build` operation and contains all necessary information for deploying the application. The file is used during the `build` operation to replace the variables in the configuration files with their actual values. 
It contains the complete configuration that was composed out of all .confixrc and .confix.repository files, except the secrets. The confix lock file is safe to commit to your repository and you have to include it in your container image. 


# Deploying your App
> **ℹ️ This section is still in development. You can leave your feedback in the [Discussions](https://github.com/SwissLife-OSS/Confix/discussions/13) about this section.**

Confix is designed to integrate into any stage of your application deployment, providing you with the flexibility to replace variables in your configuration files when and how you see fit. 
Confix does not impose a specific approach to variable replacement in configuration files. Rather, it supports various methodologies, allowing you to choose one that best suits your deployment process, security and application requirements and taste.
The following sections of the documentation provide more information about how you can utilize Confix during different stages of the deployment process: build time, deploy time, and runtime.

## Build Time
> **ℹ️ This section is still in development. You can leave your feedback in the [Discussions](https://github.com/SwissLife-OSS/Confix/discussions/14) about this section.**

When building your application's configuration, Confix provides the `confix build` command to replace the variables in your configuration files with their actual values. Confix calls the variable providers to resolve these variables. You can specify the desired environment for your build by using the `--environment` flag. 

This strategy is particularly useful when you wish to create configuration files and deploy them directly to a web app or web server.

> ⚠️ **Caution for Container Deployment**: 
If you intend to deploy your application within a container, take extra precaution with this approach. It is important to first encrypt the configuration files using a public key, and then decrypt them at runtime within the container. If you not do this means your container image will contain plaintext secrets, which could be accessed by anyone with access to the container image.

**Pros:**
1. Easy setup: The `confix build` command and the `--environment` flag are straightforward to use and it may be enough for your application's needs
2. Consistent variables across deployments: If you need to rollback a deployment, the variables remain the same as the last time you deployed the app.

**Cons:**
1. Need for encryption in containers: When deploying your application within a container, you need to encrypt your configuration files to prevent exposing secrets in the container image. 
2. You need different container images for different environments: If you need to deploy your application to multiple environments, you need to build a separate container image for each environment.

## Deploy Time
>  **ℹ️ This section is still in development. You can leave your feedback in the [Discussions](https://github.com/SwissLife-OSS/Confix/discussions/15) about this section.**

In containerized applications, you may prefer not to embed secrets in your container image at build time. Doing so could expose these secrets to anyone who gains access to the container image. Instead, you might choose to construct the container with configuration files that contain variable placeholders, replacing these with actual values when you initiate the container. 

There are primarily two methods for this approach:

1. Resolve variables upon container startup.
2. Resolve variables when you decide to deploy the app.

Details about resolving variables at runtime (method 1) are covered in the `Runtime` section. This section will focus on the second method.

### Procedure

To replace variables before deploying the application, follow these steps in your release pipeline:

1. **Prepare Variables:** Checkout the code and run the `confix prepare` command. This command comunicates with variable providers to read all necessary variables, creating a JSON file that contains just the secret values.

2. **Pass Variables to Container:** The JSON file needs to be transferred to the running container. This can be achieved by mounting the file into the container, or by using a Kubernetes secret to pass it as an environment variable.

3. **Build with Variables:** Run the `confix build` command with the `--variables </path/to/json or environment_variable>` options before initiating the container. This will substitute variables in your configuration files with their actual values.

In a Kubernetes setup, you can use an init container that has the configuration files mounted to execute the replacement command. Alternatively, you can directly integrate Confix into your Dockerfile. 

If the container's source code is inaccessible in the release pipeline, you can also run the `prepare` command in the release pipeline within inside init Docker container. When you specify the --print options, the command will print the JSON file to the standard output. You can read this output and pass it later to the container as an environment variable.

**Pros:**

1. Consistent container image across environments: This approach lets you use the same container image for all your environments.

2. Independence from variable providers: The application does not need to have access to the variable providers.

3. Tolerant to provider downtime: The variable providers can be down when you restart or rollback the app.

4. Consistent variables across deployments: You can rollback a deployment and still have the same variables as the last time you deployed the app.

**Cons:**
1. Increased complexity: You need to `prepare` the variables before deploying the app, which adds an extra layer of complexity to your release pipeline.

## Runtime

>  **ℹ️ This section is still in development. You can leave your feedback in the [Discussions](https://github.com/SwissLife-OSS/Confix/discussions/16) about this section.**

In this strategy for managing configuration, variables are resolved when the container starts up. The `confix build` command is embedded within your Dockerfile, and at startup, this command attempts to resolve the variables.

**Pros**:

1. **Dynamic Updates:** This approach provides flexibility and allows you to change a variable and then simply restart the container to apply the new value. This is advantageous when variables need to be frequently updated.
   
2. **Immediate Variable Changes:** All changes to the variables in the variable provider are immediately effective upon the next container startup, keeping your application configuration up-to-date.

**Cons**:

1. **Dependency on Variable Providers:** Your variable providers must be operational when you start the container. If they're down, the container will fail to start, causing service disruptions.
   
2. **Container Access to Providers:** The container must have access to the variable providers, meaning you must ensure the necessary permissions and network access are set up. This adds complexity to your setup.
   
3. **Rollback Inconsistencies:** When you roll back a deployment, you may not get the same variables as the ones used during the last deployment. Variables are retrieved at runtime, meaning changes to the variables in the providers after the initial deployment will be reflected in the rolled back version, potentially causing inconsistency issues.
