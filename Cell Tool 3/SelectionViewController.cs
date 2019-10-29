// This file has been autogenerated from a class added in the UI designer.

using System;

using Foundation;
using AppKit;

namespace MacControls
{
	public partial class SelectionViewController : NSViewController
	{
		#region Constructors
		public SelectionViewController (IntPtr handle) : base (handle)
		{
		}
		#endregion

		#region Override Methods
		public override void ViewWillAppear ()
		{
			base.ViewWillAppear ();

			// Wire-up controls
			TickedSlider.Activated += (sender, e) => {
				FeedbackLabel.StringValue = string.Format("Stepper Value: {0:###}",TickedSlider.IntValue);
			};

			SliderValue.Activated += (sender, e) => {
				AmountField.StringValue = string.Format("{0:###}",SliderValue.IntValue);
				AmountStepper.IntValue = SliderValue.IntValue;
			};

			AmountStepper.Activated += (sender, e) => {
				AmountField.StringValue = string.Format("{0:###}",SliderValue.IntValue);
				SliderValue.IntValue = AmountStepper.IntValue;
			};

			ColorWell.Color = NSColor.Red;
			ColorWell.Activated += (sender, e) => {
				FeedbackLabel.StringValue = string.Format("Color Changed: {0}", ColorWell.Color);
			};

			ImageWell.Image = NSImage.ImageNamed ("tag.png");
			ImageWell.Activated += (sender, e) => {
				FeedbackLabel.StringValue = "Image Well Clicked";
			};

			DateTime.Activated += (sender, e) => {
				FeedbackLabel.StringValue = DateTime.StringValue;
			};
		}
		#endregion

		#region Actions
		partial void SegmentButtonPressed (Foundation.NSObject sender) {
			FeedbackLabel.StringValue = string.Format("Button {0} Pressed",SegmentButtons.SelectedSegment);
		}

		partial void SegmentSelected (Foundation.NSObject sender) {
			FeedbackLabel.StringValue = string.Format("Segment {0} Selected",SegmentSelection.SelectedSegment);
		}
		#endregion
	}
}