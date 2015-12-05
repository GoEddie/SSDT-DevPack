using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Security.Permissions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SSDTDevPack.Clippy
{
    public enum MenuItemType
    {
        MenuItem,
        Seperator,
        Header
    }

    public struct MenuDefinition
    {
        public string Caption;
        public MenuItemType Type;
        public Action Action;

        public GlyphDefinition Glyph;
        public ClippyOperation Operation;
    }

    public partial class MainWindow : Window
    {
        public MainWindow(List<MenuDefinition> menu)
        {
            InitializeComponent();

            foreach (var definition in menu)
            {
                switch (definition.Type)
                {
                    case MenuItemType.MenuItem:
                        this.Items.Children.Add(new MenuItem(definition.Caption, definition.Action, this));
                        break;
                    case MenuItemType.Seperator:

                        this.Items.Children.Add(new TextBlock(){Height = 10, Padding = new Thickness(5,5,5,5)});
                        
                        break;
                    case MenuItemType.Header:
                        this.Items.Children.Add(new TextBlock() { Text = definition.Caption, Foreground = Brushes.Coral});
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
            }

            
            this.Deactivated += CheckClose;
        }

        void CheckClose(object sender, System.EventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception x)
            {
                
            }
        }

        public ContextMenu GetMenu()
        {
            return ContextMenu;
        }

    }
}
