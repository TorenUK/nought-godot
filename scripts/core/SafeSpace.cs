using Godot;
using System;
using System.Collections.Generic;

public partial class SafeSpace : Resource
{
    [Export] public string Name { get; set; } = "My Safe Space";
    [Export] public Godot.Collections.Array<SafeSpaceItem> Items { get; set; } = new Godot.Collections.Array<SafeSpaceItem>();
    [Export] public Vector2 Size { get; set; } = new Vector2(20, 20);
    [Export] public string Theme { get; set; } = "cozy";
    [Export] public int ComfortLevel { get; set; } = 50;
}

public partial class SafeSpaceItem : Resource
{
    [Export] public string ItemId { get; set; }
    [Export] public string Name { get; set; }
    [Export] public Vector2 Position { get; set; }
    [Export] private ItemCategory _category;
    public ItemCategory Category
    {
        get => _category;
        set => _category = value;
    }
    [Export] public int ComfortValue { get; set; }
    [Export] public string Description { get; set; }
}

public enum ItemCategory
{
    Furniture,
    Decoration,
    Plant,
    Technology,
    Exercise,
    SelfCare,
    Hobby,
    Comfort
}

public partial class SafeSpaceBuilder : Control
{
    [Export] private Node2D spaceContainer;
    [Export] private GridContainer itemPalette;
    [Export] private Label comfortLabel;
    [Export] private Button saveButton;
    [Export] private Button shareButton;

    private SafeSpace currentSpace;
    private SafeSpaceItem selectedItem;
    private bool isPlacingItem = false;
    private Dictionary<string, SafeSpaceItemData> availableItems = new();

    public override void _Ready()
    {
        currentSpace = GameManager.Instance.CurrentUser.UserSafeSpace;
        SetupAvailableItems();
        SetupItemPalette();
        LoadCurrentSpace();

        saveButton.Pressed += SaveSpace;
        // shareButton.Pressed += ShareSpace;
    }

    private void SetupAvailableItems()
    {
        availableItems = new Dictionary<string, SafeSpaceItemData>
        {
            // Furniture
            ["cozy_chair"] = new("Cozy Chair", ItemCategory.Furniture, 15, "A comfortable place to relax"),
            ["soft_bed"] = new("Soft Bed", ItemCategory.Furniture, 20, "For peaceful rest"),
            ["reading_nook"] = new("Reading Nook", ItemCategory.Furniture, 18, "Perfect for quiet moments"),
            ["meditation_cushion"] = new("Meditation Cushion", ItemCategory.Furniture, 12, "For mindful practice"),

            // Self-Care
            ["essential_oils"] = new("Essential Oils", ItemCategory.SelfCare, 10, "Calming aromatherapy"),
            ["bath_supplies"] = new("Bath Supplies", ItemCategory.SelfCare, 8, "For relaxing baths"),
            ["journal"] = new("Journal", ItemCategory.SelfCare, 7, "Write your thoughts"),
            ["tea_corner"] = new("Tea Corner", ItemCategory.SelfCare, 9, "Soothing herbal teas"),

            // Plants
            ["peace_lily"] = new("Peace Lily", ItemCategory.Plant, 6, "Air-purifying and calming"),
            ["snake_plant"] = new("Snake Plant", ItemCategory.Plant, 5, "Low maintenance greenery"),
            ["herb_garden"] = new("Herb Garden", ItemCategory.Plant, 8, "Fresh herbs for cooking"),

            // Exercise
            ["yoga_mat"] = new("Yoga Mat", ItemCategory.Exercise, 12, "For stretching and yoga"),
            ["weights"] = new("Light Weights", ItemCategory.Exercise, 10, "Strength training"),
            ["exercise_bike"] = new("Exercise Bike", ItemCategory.Exercise, 15, "Cardio at home"),

            // Hobby
            ["art_supplies"] = new("Art Supplies", ItemCategory.Hobby, 11, "Creative expression"),
            ["musical_instrument"] = new("Guitar", ItemCategory.Hobby, 14, "Make beautiful music"),
            ["puzzle_table"] = new("Puzzle Table", ItemCategory.Hobby, 9, "Mindful problem solving"),

            // Technology 
            ["meditation_app"] = new("Meditation Device", ItemCategory.Technology, 8, "Guided meditations"),
            ["e_reader"] = new("E-Reader", ItemCategory.Technology, 6, "Digital books"),

            // Comfort Items
            ["weighted_blanket"] = new("Weighted Blanket", ItemCategory.Comfort, 13, "Anxiety relief"),
            ["soft_lighting"] = new("Soft Lighting", ItemCategory.Comfort, 7, "Warm, calming light"),
            ["family_photos"] = new("Family Photos", ItemCategory.Comfort, 9, "Cherished memories")
        };
    }

    private void SetupItemPalette()
    {
        foreach (var kvp in availableItems)
        {
            var itemData = kvp.Value;
            var button = new Button();
            button.Text = itemData.Name;
            button.TooltipText = $"{itemData.Description}\nComfort: +{itemData.ComfortValue}";

            button.Pressed += () => SelectItem(kvp.Key, itemData);
            itemPalette.AddChild(button);
        }
    }

    private void SelectItem(string itemId, SafeSpaceItemData itemData)
    {
        selectedItem = new SafeSpaceItem
        {
            ItemId = itemId,
            Name = itemData.Name,
            Category = itemData.Category,
            ComfortValue = itemData.ComfortValue,
            Description = itemData.Description
        };

        isPlacingItem = true;
        Input.SetDefaultCursorShape(Input.CursorShape.Cross);
    }

    public override void _Input(InputEvent @event)
    {
        if (isPlacingItem && @event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Left)
            {
                PlaceItem(mouseButton.GlobalPosition);
            }
            else if (mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Right)
            {
                CancelPlacement();
            }
        }
    }

    private void PlaceItem(Vector2 position)
    {
        if (selectedItem == null) return;

        // Convert screen position to space coordinates
        var localPos = spaceContainer.ToLocal(position);

        // Check if position is valid (within bounds, not overlapping)
        if (IsValidPosition(localPos))
        {
            selectedItem.Position = localPos;
            currentSpace.Items.Add(selectedItem);

            CreateItemVisual(selectedItem);
            UpdateComfortLevel();

            isPlacingItem = false;
            selectedItem = null;
            Input.SetDefaultCursorShape(Input.CursorShape.Arrow);
        }
    }

    private bool IsValidPosition(Vector2 position)
    {
        // Check bounds
        if (position.X < 0 || position.Y < 0 ||
            position.X > currentSpace.Size.X || position.Y > currentSpace.Size.Y)
            return false;

        // Check for overlapping items (simplified)
        foreach (var item in currentSpace.Items)
        {
            if (item.Position.DistanceTo(position) < 2.0f)
                return false;
        }

        return true;
    }

    private void CreateItemVisual(SafeSpaceItem item)
    {
        var itemNode = new Node2D();
        itemNode.Position = item.Position;
        itemNode.Name = item.ItemId;

        // Create visual representation
        var sprite = new Sprite2D();
        sprite.Texture = GD.Load<Texture2D>($"res://items/{item.ItemId}.png");
        itemNode.AddChild(sprite);

        // Add interaction area
        var area = new Area2D();
        var collision = new CollisionShape2D();
        var shape = new RectangleShape2D();
        shape.Size = new Vector2(64, 64); // Standard item size
        collision.Shape = shape;
        area.AddChild(collision);
        itemNode.AddChild(area);

        // Connect interaction signals
        area.InputEvent += (Node viewport, InputEvent @event, long shapeIdx) =>
            OnItemClicked(item, @event);

        spaceContainer.AddChild(itemNode);
    }

    private void OnItemClicked(SafeSpaceItem item, InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
        {
            if (mouseButton.ButtonIndex == MouseButton.Right)
            {
                // Show context menu for item
                ShowItemContextMenu(item);
            }
        }
    }

    private void ShowItemContextMenu(SafeSpaceItem item)
    {
        var popup = new PopupMenu();
        popup.AddItem("Move Item");
        popup.AddItem("Remove Item");
        popup.AddItem("Item Info");

        popup.IdPressed += (long id) =>
        {
            switch (id)
            {
                case 0: // Move
                    StartMovingItem(item);
                    break;
                case 1: // Remove
                    RemoveItem(item);
                    break;
                case 2: // Info
                    ShowItemInfo(item);
                    break;
            }
        };

        AddChild(popup);
        popup.PopupOnParent(new Rect2I((int)GetGlobalMousePosition().X,
                                       (int)GetGlobalMousePosition().Y, 200, 100));
    }

    private void RemoveItem(SafeSpaceItem item)
    {
        currentSpace.Items.Remove(item);

        // Remove visual
        var itemNode = spaceContainer.GetNode(item.ItemId);
        itemNode?.QueueFree();

        UpdateComfortLevel();
    }

    private void UpdateComfortLevel()
    {
        int totalComfort = 0;
        foreach (var item in currentSpace.Items)
        {
            totalComfort += item.ComfortValue;
        }

        currentSpace.ComfortLevel = Math.Min(100, totalComfort);
        comfortLabel.Text = $"Comfort Level: {currentSpace.ComfortLevel}/100";

        // Update color based on comfort level
        Color comfortColor = currentSpace.ComfortLevel switch
        {
            >= 80 => Colors.LimeGreen,
            >= 60 => Colors.Yellow,
            >= 40 => Colors.Orange,
            _ => Colors.Red
        };

        comfortLabel.AddThemeColorOverride("font_color", comfortColor);
    }

    private void LoadCurrentSpace()
    {
        foreach (var item in currentSpace.Items)
        {
            CreateItemVisual(item);
        }
        UpdateComfortLevel();
    }

    private void SaveSpace()
    {
        // Save to user profile
        GameManager.Instance.CurrentUser.UserSafeSpace = currentSpace;
        GameManager.Instance.SaveUserData();

        // ShowMessage("Safe space saved! 🏠");
    }

    // private void ShareSpace()
    // {
    //     // Create a shareable snapshot of the space
    //     var shareData = new SafeSpaceShare
    //     {
    //         OwnerName = GameManager.Instance.CurrentUser.DisplayName,
    //         SpaceName = currentSpace.Name,
    //         ComfortLevel = currentSpace.ComfortLevel,
    //         ItemCount = currentSpace.Items.Count,
    //         Screenshot = CaptureSpaceScreenshot()
    //     };

    //     SocialManager.Instance.ShareSafeSpace(shareData);
    //     ShowMessage("Safe space shared with friends! ✨");
    // }

    private void CancelPlacement()
    {
        isPlacingItem = false;
        selectedItem = null;
        Input.SetDefaultCursorShape(Input.CursorShape.Arrow);
    }

    private void StartMovingItem(SafeSpaceItem item)
    {
        // Implementation for moving items
        selectedItem = item;
        isPlacingItem = true;
        RemoveItem(item);
    }

    private void ShowItemInfo(SafeSpaceItem item)
    {
        var dialog = new AcceptDialog();
        dialog.Title = item.Name;
        dialog.DialogText = $"{item.Description}\n\nComfort Value: +{item.ComfortValue}\nCategory: {item.Category}";
        AddChild(dialog);
        dialog.PopupCentered();
    }

    // private void ShowMessage(string message)
    // {
    //     var toast = new Label();
    //     toast.Text = message;
    //     toast.AddThemeStyleboxOverride("normal", new StyleBoxFlat
    //     {
    //         BgColor = new Color(0, 0, 0, 0.8f),
    //         CornerRadiusTopLeft = 4,
    //         CornerRadiusTopRight = 4,
    //         CornerRadiusBottomLeft = 4,
    //         CornerRadiusBottomRight = 4
    //     });
    //     toast.AddThemeColorOverride("font_color", Colors.White);
    //     toast.Position = new Vector2(10, 10);

    //     AddChild(toast);

    //     var tween = CreateTween();
    //     tween.TweenDelay(2.0);
    //     tween.TweenCallback(Callable.From(() => toast.QueueFree()));
    // }

    private ImageTexture CaptureSpaceScreenshot()
    {
        // Simplified screenshot capture
        var viewport = GetViewport();
        var img = viewport.GetTexture().GetImage();
        var texture = ImageTexture.CreateFromImage(img);
        return texture;
    }
}

public class SafeSpaceItemData
{
    public string Name { get; set; }
    public ItemCategory Category { get; set; }
    public int ComfortValue { get; set; }
    public string Description { get; set; }

    public SafeSpaceItemData(string name, ItemCategory category, int comfortValue, string description)
    {
        Name = name;
        Category = category;
        ComfortValue = comfortValue;
        Description = description;
    }
}

public class SafeSpaceShare
{
    public string OwnerName { get; set; }
    public string SpaceName { get; set; }
    public int ComfortLevel { get; set; }
    public int ItemCount { get; set; }
    public ImageTexture Screenshot { get; set; }
    public DateTime SharedAt { get; set; } = DateTime.Now;
}