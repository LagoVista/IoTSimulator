using LagoVista.XPlat.Core;
using Xamarin.Forms.Xaml;

namespace LagoVista.Simulator.Views.Messages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SendMessageView : LagoVistaContentPage
    {
        public SendMessageView()
        {
            InitializeComponent();
            AttributeList.ItemSelected += (sndr, args) => AttributeList.SelectedItem = null;
        }        
    }
}