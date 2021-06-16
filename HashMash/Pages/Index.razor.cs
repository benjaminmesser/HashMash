namespace HashMash.Pages
{
    public partial class Index
    {
        // Flag to show the welcome page or not - default is false (show alert)
        private bool _continued => true ? sessionStorage.GetItem<bool>("dismissed") : false;
    }
}
