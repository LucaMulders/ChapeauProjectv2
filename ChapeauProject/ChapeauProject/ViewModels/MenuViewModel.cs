using System.Collections.Generic;
using ChapeauProject.Models;

namespace ChapeauProject.ViewModels
{
    public class MenuViewModel
    {
        public List<MenuItem> MenuItems { get; set; }
        public List<string> Categories { get; set; }
        public string SelectedCard { get; set; }
        public string SelectedCategory { get; set; }
    }
}