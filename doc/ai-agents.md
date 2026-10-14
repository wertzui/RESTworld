# Using RESTworld with AI coding agents

RESTworld ships agent skills for developers who build applications **with** RESTworld. They teach an AI coding agent the project layout, the registration calls, the DTO and mapper rules and the Angular client, so it generates code that fits the framework. The skills are listed in the [How-To Cookbook](how-to.md).

## Install as a Copilot plugin (recommended)

The repository is a plugin marketplace. In Copilot CLI:

```shell
copilot plugin marketplace add wertzui/RESTworld
copilot plugin install restworld@restworld
```

You can also install the plugin directly, without registering the marketplace:

```shell
copilot plugin install wertzui/RESTworld:plugins/restworld
```

Update it later with `copilot plugin update restworld`.

To enable the plugin for everyone who works in your repository, including Copilot cloud agent, add it to `.github/copilot/settings.json`:

```json
{
  "extraKnownMarketplaces": { "restworld": { "source": { "source": "github", "repo": "wertzui/RESTworld" } } },
  "enabledPlugins": { "restworld@restworld": true }
}
```

Check the exact shape of these two settings in the [Copilot plugin documentation](https://docs.github.com/en/copilot/concepts/agents/about-plugins) for your Copilot version.

In VS Code, enable agent plugins with the `chat.plugins.enabled` setting and add the marketplace from the plugins view. The plugin uses the portable [Agent Plugins 1.0](https://agent-plugins.org/) format, so other compatible clients can load it too.

## Install the skills manually

If your agent has no plugin support, copy the folders from [`plugins/restworld/skills`](../plugins/restworld/skills) to the place your agent scans for skills, for example:

| Agent | Folder in your repository |
|---|---|
| GitHub Copilot (VS Code, CLI) | `.github/skills/` |
| Claude Code | `.claude/skills/` |
| Other agents following the Agent Skills format | `.agents/skills/` or the folder named in the agent's documentation |

Restart the agent session afterwards. Copy all folders together, because the skills link to each other.

## Add the instruction file

Copy [`AGENTS.md`](../src/Example/ExampleBlog/AGENTS.md) to the root of your solution. It describes the solution layout and the rules for RESTworld projects and names the skill for each task. Most agents read it automatically, even if they do not support skills.

## Working with the skills

- Start with `restworld-add-entity-endpoint` for a new endpoint. It calls the other skills in order.
- If the agent does not pick a skill, name it: "use the `restworld-custom-service` skill".
- Skills load on demand, instructions load every time. Keep your own `AGENTS.md` short.
- The skills link to the documentation on GitHub, so the agent needs web access to follow those links. The skill text itself is self-contained.