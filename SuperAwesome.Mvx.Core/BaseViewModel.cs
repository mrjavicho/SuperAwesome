using System;
using System.Threading.Tasks;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;

namespace SuperAwesome.Mvx.Forms.Core
{
    public abstract class BaseViewModel : MvxViewModel
    {
        protected bool _isBusy;
        private bool _firstTimeAppearing = true;

        protected IMvxNavigationService _navigationService { get; }

        protected BaseViewModel(IMvxNavigationService navigationService) : base()
        {
            _navigationService = navigationService;
        }

        public override void ViewAppearing()
        {
            System.Diagnostics.Debug.WriteLine($"Showing: {this.GetType().Name}");

            if (_firstTimeAppearing)
            {
                FirstTimeAppearing();
                _firstTimeAppearing = false;
            }

            base.ViewAppearing();
        }

        protected virtual void FirstTimeAppearing()
        {
        }

        protected async Task ExecuteTask(Func<Task> action)
        {
            try
            {
                if (_isBusy)
                {
                    return;
                }

                _isBusy = true;
                await action.Invoke();
            }
            catch (Exception e)
            {
                await HandleException(e);
            }
            finally
            {
                _isBusy = false;
            }
        }

        protected async Task<TResult> ExecuteTask<TResult>(Func<Task<TResult>> action)
        {
            try
            {
                if (_isBusy)
                {
                    return default;
                }

                _isBusy = true;
                await action.Invoke();
            }
            catch (Exception e)
            {
                await HandleException(e);
            }
            finally
            {
                _isBusy = false;
            }

            return default;
        }

        protected virtual Task HandleException(Exception e)
        {
            System.Diagnostics.Debug.WriteLine($"{this.GetType().Name} : {e.Message}");
            System.Diagnostics.Debug.WriteLine(e.StackTrace);
            return Task.CompletedTask;
        }
    }

    public abstract class BaseViewModel<TParameter> : BaseViewModel, IMvxViewModel<TParameter>
    {
        protected BaseViewModel(IMvxNavigationService navigationService) : base(navigationService)
        {
        }

        public abstract void Prepare(TParameter parameter);
    }

    public abstract class BaseViewModelResult<TResult> : BaseViewModel, IMvxViewModelResult<TResult>
    {
        protected BaseViewModelResult(IMvxNavigationService navigationService) : base(navigationService)
        {
        }

        public TaskCompletionSource<object> CloseCompletionSource { get; set; }

        public override void ViewDestroy(bool viewFinishing = true)
        {
            if (viewFinishing && CloseCompletionSource != null && !CloseCompletionSource.Task.IsCompleted &&
                !CloseCompletionSource.Task.IsFaulted)
                CloseCompletionSource?.TrySetCanceled();
            base.ViewDestroy(viewFinishing);
        }
    }

    public abstract class BaseViewModel<TParameter, TResult> : BaseViewModel, IMvxViewModel<TParameter, TResult>
    {
        protected BaseViewModel(IMvxNavigationService navigationService) : base(navigationService)
        {
        }

        public abstract void Prepare(TParameter parameter);

        public TaskCompletionSource<object> CloseCompletionSource { get; set; }

        public override void ViewDestroy(bool viewFinishing = true)
        {
            if (viewFinishing && CloseCompletionSource != null && !CloseCompletionSource.Task.IsCompleted &&
                !CloseCompletionSource.Task.IsFaulted)
                CloseCompletionSource?.TrySetCanceled();
            base.ViewDestroy(viewFinishing);
        }
    }
}