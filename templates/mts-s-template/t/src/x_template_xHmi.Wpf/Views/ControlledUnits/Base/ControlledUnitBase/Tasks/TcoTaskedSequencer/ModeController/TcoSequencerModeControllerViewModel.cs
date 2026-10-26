using TcoCore;
using Vortex.Presentation.Wpf;

namespace x_template_xPlc
{
    public class TcoSequencerModeControllerViewModel : RenderableViewModel
    {
        public TcoSequencerModeControllerViewModel()
        {
            
        }

        public TcoSequencerModeController TcoSequencerModeController { get; private set; }

        public override object Model { get => TcoSequencerModeController; set { TcoSequencerModeController = value as TcoSequencerModeController; } }

        
    }
}
