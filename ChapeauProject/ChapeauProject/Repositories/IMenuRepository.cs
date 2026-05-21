using ChapeauProject.Models;
using System.Collections.Generic;

namespace ChapeauProject.Repositories
{
    public interface IMenuRepository
    {
        // Filters by card (Lunch/Dinner) and course (Starters/Mains/etc.) 
        List<MenuItem> GetFiltered(string cardFilter, string courseFilter); //NOTE change to enum
    }
}