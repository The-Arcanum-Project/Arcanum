namespace Arcanum.API.UtilServices;

/// <summary>
/// An interface all Services registered with the plugin host must implement to allow for proper unloading and cleanup.
/// </summary>
public interface IService
{
   /// <summary>
   /// Performs any necessary cleanup or resource deallocation
   /// when the service is being unloaded by the plugin host.
   /// </summary>
   public void Unload();

   /// <summary>
   /// Verifies the current state of the service, ensuring it is operational and consistent.
   /// This method returns an enumeration indicating the service's state.
   /// </summary>
   /// <returns>
   /// Returns a <see cref="IService.ServiceState"/> value representing the operational state of the service.
   /// Possible values include Ok, Error, or Unknown.
   /// </returns>
   public ServiceState VerifyState();

   public enum ServiceState
   {
      /// <summary>
      /// The service is in a valid and operational state,
      /// </summary>
      Ok,

      /// <summary>
      /// The service is in an error state, possibly due to exceptions or misconfiguration.
      /// </summary>
      Error,

      /// <summary>
      /// The service is in an unknown state, possibly due to initialization issues or other problems.
      /// </summary>
      Unknown,
   }
}