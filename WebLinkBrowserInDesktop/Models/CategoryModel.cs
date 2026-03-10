using System.Collections.ObjectModel;

namespace WebLinkBrowserInDesktop.Models
{
    public class CategoryModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentId { get; set; }
        public ObservableCollection<object> Items { get; set; } = new ObservableCollection<object>();
    }
}
