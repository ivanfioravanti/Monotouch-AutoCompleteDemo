using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Linq;
using System.Threading;

namespace AutoCompleteDemo {

	public partial class AutoCompleteDemoViewController : UIViewController {
		UITableView autoCompleteTable;

		string[] wordCollection = {"blah", "bleh", "blop", "boing", "derp", "deep"};

		public AutoCompleteDemoViewController () : base ("AutoCompleteDemoViewController", null){
		}
		
		public override void DidReceiveMemoryWarning () {
			base.DidReceiveMemoryWarning ();
		}
		
		public override void ViewDidLoad () {
			base.ViewDidLoad ();

			autoCompleteTable = new UITableView (new RectangleF (0,79,
			                                                     UIScreen.MainScreen.Bounds.Width,
			                                                     UIScreen.MainScreen.Bounds.Height-79))
			{
				AutoresizingMask = UIViewAutoresizing.FlexibleRightMargin | 
					UIViewAutoresizing.FlexibleBottomMargin |
					UIViewAutoresizing.FlexibleWidth,
				ScrollEnabled = true,
				BackgroundColor = UIColor.White,
				SeparatorColor = UIColor.Gray,
				Hidden = true
			};

			this.View.AddSubview(autoCompleteTable);
			textInput.ShouldReturn = delegate
			{
				textInput.ResignFirstResponder();
				return true;
			};
			textInput.ShouldChangeCharacters += (sender, something, e) => {
				Thread autoCompleteThread = new Thread (() => {
					UpdateSuggestions();
				});
				autoCompleteThread.Start ();
				return true;
			};
		}

		public void UpdateSuggestions() {
			string[] suggestions = null;

			try {
				InvokeOnMainThread(() => {
					suggestions = wordCollection.Where (x => x.ToLowerInvariant().Contains(textInput.Text))
						.OrderByDescending(x => x.ToLowerInvariant().StartsWith(textInput.Text))
						.Select (x => x).ToArray();
				});

				if (suggestions.Length != 0) {
					InvokeOnMainThread(() => {
						autoCompleteTable.Hidden = false;
						autoCompleteTable.Source = new AutoCompleteTableSource(suggestions, this);
						autoCompleteTable.ReloadData();
					});
				} else {
					InvokeOnMainThread(() => {
						autoCompleteTable.Hidden = true;
					});
				}
			} catch(Exception) {
				Console.WriteLine("Error: Can't retrieve suggestions");
			}
		}

		public void SetAutoCompleteText(string finalString) {
			textInput.Text = finalString;
			textInput.ResignFirstResponder();
			autoCompleteTable.Hidden = true;

			labelSelection.Text = "You selected: " + finalString;
			labelSelection.Hidden = false;
		}
		
		public override void ViewDidUnload () {
			base.ViewDidUnload ();
			ReleaseDesignerOutlets ();
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation) {
			return (toInterfaceOrientation != UIInterfaceOrientation.PortraitUpsideDown);
		}
	}
}

