using DrawGuess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DrawGuess.TemplateSelectors
{
    public class GameDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate NewGameTemplate { get; set; }
        public DataTemplate GameTemplate { get; set; }
        public DataTemplate FullGameTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            Game game = (Game)item;

            DataTemplate _returnTemplate = new DataTemplate();
            var itemsControl = ItemsControl.ItemsControlFromItemContainer(container);

            if (itemsControl.IndexFromContainer(container) == 0)
            {
                _returnTemplate = NewGameTemplate;
            }
            else if (game.Full)
            {
                _returnTemplate = FullGameTemplate;
            }
            else
            {
                _returnTemplate = GameTemplate;
            }

            return _returnTemplate;
        }
    }
}
