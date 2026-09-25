using StorageInventory.Application;
using StorageInventory.Domain;
using System.Drawing;

namespace StorageInventory;

public class MainForm : Form
{
    private readonly InventoryService _service;
    private List<Box> _boxes = new();

    private readonly ListBox _boxList = new();
    private readonly DataGridView _itemsGrid = new();
    private readonly TextBox _boxNameTextBox = new();
    private readonly TextBox _aisleTextBox = new();
    private readonly TextBox _shelfTextBox = new();
    private readonly TextBox _itemNameTextBox = new();
    private readonly NumericUpDown _quantityUpDown = new();
    private readonly Label _statusLabel = new();

    private Guid? _selectedBoxId;

    public MainForm(InventoryService service)
    {
        _service = service;

        Text = "Storage Inventory";
        StartPosition = FormStartPosition.CenterScreen;
        Width = 1100;
        Height = 750;
        MinimumSize = new Size(900, 650);

        BuildLayout();
        Load += async (_, _) => await RefreshBoxesAsync();
    }

    private void BuildLayout()
    {
        var root = new SplitContainer
        {
            Dock = DockStyle.Fill,
            SplitterDistance = 320,
            FixedPanel = FixedPanel.Panel1,
            IsSplitterFixed = false,
            Panel1MinSize = 280,
            Panel2MinSize = 520
        };

        BuildLeftPanel(root.Panel1);
        BuildRightPanel(root.Panel2);

        Controls.Add(root);
    }

    private void BuildLeftPanel(Control parent)
    {
        var title = new Label
        {
            Text = "Boxes",
            Dock = DockStyle.Top,
            Height = 32,
            Font = new Font(Font.FontFamily, 14, FontStyle.Bold),
            Padding = new Padding(8, 6, 8, 0)
        };

        _boxList.Dock = DockStyle.Fill;
        _boxList.DisplayMember = nameof(Box.Name);
        _boxList.SelectedIndexChanged += async (_, _) => await OnBoxSelectionChangedAsync();

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 52,
            Padding = new Padding(8),
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };

        var refreshButton = new Button
        {
            Text = "Refresh",
            Width = 100,
            Height = 32
        };
        refreshButton.Click += async (_, _) => await RefreshBoxesAsync();

        var deleteButton = new Button
        {
            Text = "Delete Box",
            Width = 120,
            Height = 32
        };
        deleteButton.Click += async (_, _) => await DeleteSelectedBoxAsync();

        buttonPanel.Controls.Add(refreshButton);
        buttonPanel.Controls.Add(deleteButton);

        parent.Controls.Add(_boxList);
        parent.Controls.Add(buttonPanel);
        parent.Controls.Add(title);
    }

    private void BuildRightPanel(Control parent)
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(12)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 160));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

        layout.Controls.Add(BuildCreateBoxGroup(), 0, 0);
        layout.Controls.Add(BuildItemGroup(), 0, 1);
        layout.Controls.Add(BuildItemsGrid(), 0, 2);

        _statusLabel.Dock = DockStyle.Fill;
        _statusLabel.Text = "Ready.";
        _statusLabel.Padding = new Padding(4, 6, 4, 0);
        layout.Controls.Add(_statusLabel, 0, 3);

        parent.Controls.Add(layout);
    }

    private Control BuildCreateBoxGroup()
    {
        var group = new GroupBox
        {
            Text = "Add new box",
            Dock = DockStyle.Fill
        };

        var layout = CreateFormGrid(3);

        AddLabeledControl(layout, 0, "Name", _boxNameTextBox);
        AddLabeledControl(layout, 1, "Aisle", _aisleTextBox);
        AddLabeledControl(layout, 2, "Shelf", _shelfTextBox);

        var addButton = new Button
        {
            Text = "Add Box",
            Dock = DockStyle.Right,
            Width = 120
        };
        addButton.Click += async (_, _) => await AddBoxAsync();

        layout.Controls.Add(addButton, 1, 3);

        group.Controls.Add(layout);
        return group;
    }

    private Control BuildItemGroup()
    {
        var group = new GroupBox
        {
            Text = "Add or remove item from selected box",
            Dock = DockStyle.Fill
        };

        var layout = CreateFormGrid(3);

        _itemNameTextBox.Dock = DockStyle.Fill;
        AddLabeledControl(layout, 0, "Item name", _itemNameTextBox);

        _quantityUpDown.Dock = DockStyle.Left;
        _quantityUpDown.Minimum = 1;
        _quantityUpDown.Maximum = 100000;
        _quantityUpDown.Value = 1;
        AddLabeledControl(layout, 1, "Quantity", _quantityUpDown);

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 2, 0, 0)
        };

        var addItemButton = new Button
        {
            Text = "Add Item",
            Width = 120,
            Height = 32
        };
        addItemButton.Click += async (_, _) => await AddItemAsync();

        var removeItemButton = new Button
        {
            Text = "Remove Item",
            Width = 120,
            Height = 32
        };
        removeItemButton.Click += async (_, _) => await RemoveItemAsync();

        buttonPanel.Controls.Add(addItemButton);
        buttonPanel.Controls.Add(removeItemButton);
        layout.Controls.Add(buttonPanel, 1, 3);

        group.Controls.Add(layout);
        return group;
    }

    private Control BuildItemsGrid()
    {
        var group = new GroupBox
        {
            Text = "Selected box items",
            Dock = DockStyle.Fill
        };

        _itemsGrid.Dock = DockStyle.Fill;
        _itemsGrid.ReadOnly = true;
        _itemsGrid.AllowUserToAddRows = false;
        _itemsGrid.AllowUserToDeleteRows = false;
        _itemsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _itemsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _itemsGrid.RowHeadersVisible = false;

        group.Controls.Add(_itemsGrid);
        return group;
    }

    private static TableLayoutPanel CreateFormGrid(int rows)
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = rows + 1,
            Padding = new Padding(10)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        for (var i = 0; i < rows; i++)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        }

        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        return layout;
    }

    private static void AddLabeledControl(TableLayoutPanel layout, int row, string labelText, Control control)
    {
        var label = new Label
        {
            Text = labelText,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(0, 6, 0, 0)
        };

        control.Dock = DockStyle.Fill;
        layout.Controls.Add(label, 0, row);
        layout.Controls.Add(control, 1, row);
    }

    private async Task RefreshBoxesAsync()
    {
        try
        {
            var previousSelection = _selectedBoxId;
            _boxes = (await _service.GetAllAsync()).OrderBy(box => box.Name).ToList();

            _boxList.BeginUpdate();
            _boxList.DataSource = null;
            _boxList.DataSource = _boxes;
            _boxList.EndUpdate();

            if (previousSelection is Guid selectedId)
            {
                var index = _boxes.FindIndex(box => box.Id == selectedId);
                if (index >= 0)
                {
                    _boxList.SelectedIndex = index;
                }
            }

            if (_boxList.SelectedIndex < 0 && _boxes.Count > 0)
            {
                _boxList.SelectedIndex = 0;
            }

            if (_boxes.Count == 0)
            {
                _itemsGrid.DataSource = null;
                _selectedBoxId = null;
            }

            SetStatus($"Loaded {_boxes.Count} box(es).");
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private async Task OnBoxSelectionChangedAsync()
    {
        if (_boxList.SelectedItem is not Box selectedBox)
        {
            _selectedBoxId = null;
            _itemsGrid.DataSource = null;
            return;
        }

        _selectedBoxId = selectedBox.Id;
        await ShowSelectedBoxItemsAsync(selectedBox.Id);
    }

    private async Task ShowSelectedBoxItemsAsync(Guid boxId)
    {
        var box = (await _service.GetAllAsync()).FirstOrDefault(x => x.Id == boxId);
        if (box is null)
        {
            _itemsGrid.DataSource = null;
            return;
        }

        _itemsGrid.DataSource = box.Items
            .Select(item => new { item.Name, item.Quantity })
            .ToList();

        SetStatus($"Selected box: {box.Name}");
    }

    private async Task AddBoxAsync()
    {
        var name = _boxNameTextBox.Text.Trim();
        var aisle = _aisleTextBox.Text.Trim();
        var shelf = _shelfTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(aisle) || string.IsNullOrWhiteSpace(shelf))
        {
            ShowError("Name, aisle, and shelf are required.");
            return;
        }

        try
        {
            await _service.CreateBoxAsync(name, aisle, shelf);
            _boxNameTextBox.Clear();
            _aisleTextBox.Clear();
            _shelfTextBox.Clear();
            await RefreshBoxesAsync();
            SetStatus($"Box '{name}' added.");
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private async Task AddItemAsync()
    {
        var selectedBox = GetSelectedBox();
        if (selectedBox is null)
        {
            ShowError("Select a box first.");
            return;
        }

        var itemName = _itemNameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(itemName))
        {
            ShowError("Item name is required.");
            return;
        }

        try
        {
            await _service.AddItemToBoxAsync(selectedBox.Id, itemName, (int)_quantityUpDown.Value);
            _itemNameTextBox.Clear();
            _quantityUpDown.Value = 1;
            await RefreshBoxesAsync();
            SetStatus($"Item '{itemName}' updated.");
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private async Task RemoveItemAsync()
    {
        var selectedBox = GetSelectedBox();
        if (selectedBox is null)
        {
            ShowError("Select a box first.");
            return;
        }

        var itemName = _itemNameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(itemName))
        {
            ShowError("Item name is required.");
            return;
        }

        try
        {
            await _service.RemoveItemFromBoxAsync(selectedBox.Id, itemName, (int)_quantityUpDown.Value);
            await RefreshBoxesAsync();
            SetStatus($"Item '{itemName}' removed.");
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private async Task DeleteSelectedBoxAsync()
    {
        var selectedBox = GetSelectedBox();
        if (selectedBox is null)
        {
            ShowError("Select a box first.");
            return;
        }

        var result = MessageBox.Show(
            $"Delete box '{selectedBox.Name}'?",
            "Confirm delete",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result != DialogResult.Yes)
        {
            return;
        }

        try
        {
            await _service.DeleteBoxAsync(selectedBox.Id);
            _selectedBoxId = null;
            await RefreshBoxesAsync();
            SetStatus($"Box '{selectedBox.Name}' deleted.");
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private Box? GetSelectedBox()
    {
        return _boxList.SelectedItem as Box;
    }

    private void SetStatus(string message)
    {
        _statusLabel.Text = message;
    }

    private void ShowError(string message)
    {
        MessageBox.Show(this, message, "Storage Inventory", MessageBoxButtons.OK, MessageBoxIcon.Error);
        SetStatus(message);
    }
}
