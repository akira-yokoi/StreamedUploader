using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;

namespace STREAMED
{
  class ViewUtil
  {
    public static void goHome( Frame frame)
    {
      IList<PageStackEntry> backstack = frame.BackStack;
      int stackCount = backstack.Count;
      for (int popCount = 0; popCount < stackCount; popCount++)
      {
        frame.GoBack();
      }
    }

    public static async void showMesssage(String message)
    {
      await new MessageDialog(message).ShowAsync();
    }
  }
}
