using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DrawGuess.TemplateSelectors
{
    public class LetterTemplateSelector : DataTemplateSelector
    {
        public DataTemplate MissingLetterTemplate { get; set; }
        public DataTemplate SpacingTemplate { get; set; }
        public DataTemplate LetterTemplate { get; set; }
        

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            string letter = item.ToString();

            DataTemplate _returnTemplate = new DataTemplate();
            var itemsControl = ItemsControl.ItemsControlFromItemContainer(container);

            if (letter.Equals(""))
            {
                _returnTemplate = MissingLetterTemplate;
            }
            else if (letter.Equals(" "))
            {
                _returnTemplate = SpacingTemplate;
            }
            else
            {
                _returnTemplate = LetterTemplate;
            }

            return _returnTemplate;
        }
    }
}
