# Rvne.DerivedContainers

A reflection-powered tree builder for extracting typed nodes from nested object graphs.

Supports:

- Deriving `Tree<T>` from public fields and properties
- Mapping node values while preserving the original tree shape
- Cycle-safe traversal with pruning of branches that contain no matches

With a small API to help you inspect and transform nested structures

```csharp
using Rvne.DerivedContainers;

Screen screen = new()
{
    Root = new Panel("Root")
    {
        Header = new Panel("Header")
        {
            Left = new Label("Inventory"),
            Right = new Button("Close")
        },
        Body = new Panel("Body")
        {
            Left = new Button("Craft"),
            Right = new StatsCard()
        },
        Footer = new Panel("Footer")
        {
            Left = new Label("v1.0.0"),
            Right = new Button("Settings")
        }
    },
    Theme = new Theme { Name = "Amber" },                     // Filtered out
    Analytics = new AnalyticsState { LastFrameTimeMs = 16.4 } // Filtered out
};

// Keep only objects assignable to IRenderable.
Tree<IRenderable> renderTree = Tree<IRenderable>.DeriveFrom(screen);

// Run a render pass over the same shape by passing a closure.
Tree<RenderResult> frame = renderTree.Map(node => node.Render(renderer));

// Project the same tree into display names.
Tree<string> names = renderTree.Map(node => node.DebugName);
```
