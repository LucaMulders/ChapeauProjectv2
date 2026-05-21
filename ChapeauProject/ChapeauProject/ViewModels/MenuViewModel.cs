using ChapeauProject.Models;
using System.Collections.Generic;

namespace ChapeauProject.ViewModels
{
    public class MenuViewModel
    {
        // actual objects since we are dealing with oop
        public List<MenuItem> MenuItems { get; set; }
        public List<string> Categories { get; set; }
        public string SelectedCard { get; set; }
        public string SelectedCategory { get; set; }
    }
}