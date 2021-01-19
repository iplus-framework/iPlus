using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    /// <summary>
    /// PARole is the base class for so-called "manager classes", which are typically configured under the address "LocalServiceObjects". Derivatives of PARole must always be programmed as a stateless ACComponent. All necessary data must be passed by parameters. "Manager Classes" assume the role of a "central management" that provides shared program code for different classes (Businesobjects, Workflowclasses...).
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'PARole'}de{'PARole'}", Global.ACKinds.TPARole, Global.ACStorableTypes.Required, false, true)]
    public abstract class PARole : PAClassAlarmingBase
    {
        #region c'tors
        public PARole(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }
        #endregion

        #region Properties
        public override bool IsPoolable
        {
            get
            {
                return true;
            }
        }
        #endregion

        #region Enums
        public enum ValidationBehaviour
        {
            Laxly = 0,
            Strict = 1
        }

        /// <summary>Controls the behaviour how a instance of a Managerclass will be returned from method GetServiceInstance</summary>
        [Flags]
        public enum CreationBehaviour
        {
            /// <summary>
            /// <para>1: Return if configured as a child that match passed acIdentifier. If not then</para>
            /// <para>2: return if configured at ACUrl "\LocalServiceObjects" that match passed acIdentifier. If not then</para>
            /// <para>3: return if configured as a network-service at ACUrl "\Service" that match passed acIdentifier.</para>
            /// </summary>
            Default = 0x0,

            /// <summary>
            /// Same as "Default" without step 3: Don't return it as a network-service at ACUrl "\Service"
            /// </summary>
            OnlyLocal = 0x1,

            /// <summary>
            /// <para>Same as "Default". If instance could not found via passed acIdentifier then a child will be searched in step 1) and 2), that is of type &lt;T&gt;.</para>
            /// <para>If child was also not found at "\LocalServiceObjects" a new instance of type &lt;T&gt; will be created automatically.</para>
            /// </summary>
            FirstOrDefault = 0x2,
        }
        #endregion

        #region Reference-Handling
        /// <summary>Returns a instance of a "Manager-Class". The search depends on the passed CreationBehaviour parameter.</summary>
        /// <typeparam name="T">Type of class that ist derived from PARole. Otherwise if a network-Instance is searched, than T is IACComponent.</typeparam>
        /// <param name="requester">Instance that retrieves the service instance ("Manager-Class" or PARole)</param>
        /// <param name="acIdentifier">The ACIdentifier of the service instance whe this method should search for.</param>
        /// <param name="creationBehaviour">The creation behaviour.</param>
        /// <returns>A instance to a PARole or IACComponent (if network service)</returns>
        public static T GetServiceInstance<T>(ACComponent requester, string acIdentifier, CreationBehaviour creationBehaviour = CreationBehaviour.Default) where T : IACComponent
        {
            if (requester == null)
                return default(T);

            T serviceInstance;
            // 1. If configured as child:
            if (creationBehaviour.HasFlag(CreationBehaviour.FirstOrDefault))
                serviceInstance = (T) requester.ACUrlCommand(ACUrlHelper.Delimiter_Exists + acIdentifier);
            else
                serviceInstance = (T) requester.ACUrlCommand(acIdentifier);
            if (serviceInstance != null)
                return serviceInstance;

            if (creationBehaviour.HasFlag(CreationBehaviour.FirstOrDefault))
            {
                serviceInstance = requester.FindChildComponents<T>(c => c is T, null, 1).FirstOrDefault();
                if (serviceInstance != null)
                    return serviceInstance;
            }

            if (   requester.Root == null 
                || requester.Root.InitState == ACInitState.Destructing 
                || requester.Root.InitState == ACInitState.Destructed)
                return default(T);

            // 2. If configured as local service object
            LocalServiceObjects localServiceObjects = requester as LocalServiceObjects;
            if (localServiceObjects == null)
                localServiceObjects = requester.ACUrlCommand(ACUrlHelper.Delimiter_DirSeperator + LocalServiceObjects.ClassName) as LocalServiceObjects;

            if (localServiceObjects != null)
            {
                serviceInstance = (T) localServiceObjects.ACUrlCommand(acIdentifier);
                if (   serviceInstance == null 
                    && creationBehaviour.HasFlag(CreationBehaviour.FirstOrDefault))
                {
                    string className = typeof(T).Name;
                    try
                    {
                        serviceInstance = (T) localServiceObjects.ACUrlCommand(className);
                    }
                    catch (Exception e)
                    {
                        requester.Root.Messages.LogException(requester.GetACUrl(), "PARole.GetServiceInstance(10)", e);
                    }

                    if (serviceInstance == null)
                    {
                        try
                        {
                            ACClass acClass = gip.core.datamodel.Database.GlobalDatabase.GetACType(className);
                            if (acClass != null)
                                serviceInstance = (T) localServiceObjects.StartComponent(acClass, null, null);
                        }
                        catch (Exception e)
                        {
                            requester.Root.Messages.LogException(requester.GetACUrl(), "PARole.GetServiceInstance(20)", e);
                        }
                    }
                }
            }

            if (serviceInstance != null)
                return serviceInstance;

            if (creationBehaviour.HasFlag(CreationBehaviour.OnlyLocal))
                return default(T);

            // 3. If configured as network service
            serviceInstance = (T) requester.ACUrlCommand(ACUrlHelper.Delimiter_DirSeperator + LocalServiceObjects.ClassNameNetService + ACUrlHelper.Delimiter_DirSeperator + acIdentifier);
            return serviceInstance;
        }


        /// <summary>Returns the smart-pointer ACRef to a instance of a "Manager-Class". The search depends on the passed CreationBehaviour parameter.</summary>
        /// <typeparam name="T">Type of class that ist derived from PARole. Otherwise if a network-Instance is searched, than T is IACComponent.</typeparam>
        /// <param name="requester">Instance that retrieves the service instance ("Manager-Class" or PARole) and is set as the ParentACComponent in ACRef.</param>
        /// <param name="acIdentifier">The ACIdentifier of the service instance whe this method should search for.</param>
        /// <param name="creationBehaviour">The creation behaviour.</param>
        /// <returns>A reference to a PARole or IACComponent (if network service)</returns>
        public static ACRef<T> ACRefToServiceInstance<T>(ACComponent requester, string acIdentifier, CreationBehaviour creationBehaviour = CreationBehaviour.Default) where T : IACComponent
        {
            T serviceInstance = GetServiceInstance<T>(requester, acIdentifier, creationBehaviour);
            if (serviceInstance != null)
                return new ACRef<T>(serviceInstance, requester);
            return null;
        }


        /// <summary>Detaches the ACRef&lt;T&gt; from the service instance.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requester">Instance that retrieves the service instance ("Manager-Class" or PARole) and is set as the ParentACComponent in ACRef.</param>
        /// <param name="acRef">The reference that was originally created from method ACRefToServiceInstance()</param>
        public static void DetachACRefFromServiceInstance<T>(ACComponent requester, ACRef<T> acRef) where T : IACComponent
        {
            if (acRef == null)
                return;
            T serviceInstance = (T) acRef.ValueT;
            acRef.Detach();
            if (serviceInstance == null)
                return;
            if (serviceInstance.ParentACComponent == (requester.Root as ACRoot).LocalServiceObjects)
            {
                if (   !serviceInstance.ReferencePoint.HasStrongReferences
                    && serviceInstance.ComponentClass != null
                    && serviceInstance.ComponentClass.ACStartType == Global.ACStartTypes.AutomaticOnDemand)
                    serviceInstance.Stop();
            }
        }

        #endregion
    }
}
