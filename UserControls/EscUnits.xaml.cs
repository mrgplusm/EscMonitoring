using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Monitoring.UserControls
{
    /// <summary>
    /// Interaction logic for MoEscUnits.xaml
    /// </summary>
    public partial class EscUnits : UserControl
    {
        readonly Style _unitStyle = new Style();

        public EscUnits()
        {
            _unitStyle.Setters.Add(
                new Setter
                    {
                        Property = EffectProperty,
                        Value = new DropShadowEffect
                                    {
                                        Color = Colors.Black,
                                        BlurRadius = 10
                                    },
                    });

            InitializeComponent();

            //var node1 = new Node() { Width = 50, Height = 50 };
            //var node2 = new Node() { Width = 50, Height = 50 };

            //node1.Style = _unitStyle;
            //node2.Style = _unitStyle;

            //Canvas.SetLeft(node1, 0);
            //Canvas.SetLeft(node2, 200);

            //Canvas.SetTop(node1, 0);
            //Canvas.SetTop(node2, 0);

            //var conn = new Connector
            //               {
            //                   Source = node1.AnchorPoint,
            //                   Destination = node2.AnchorPoint,
            //                   BorderBrush = Brushes.Wheat,
            //               };

            

            //myCanvas.Children.Add(node1);
            //myCanvas.Children.Add(node2);

            //myCanvas.Children.Add(conn);

            //conn.SetBinding(Connector.SourceProperty,
            //    new Binding
            //        {
            //            Source = node1,
            //            Path = new PropertyPath(Node.AnchorPointProperty)
            //        });


            //conn.SetBinding(Connector.DestinationProperty,
            //                new Binding
            //                    {
            //                        Source = node2,
            //                        Path = new PropertyPath(Node.AnchorPointProperty)
            //                    });
        }
    }
}
