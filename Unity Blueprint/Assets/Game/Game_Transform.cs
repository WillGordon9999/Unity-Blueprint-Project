

namespace Game
{
    public class Transform : UnityEngine.Transform, IGameComponent
    {
        public Transform()
        {

        }

        public System.Action<UnityEngine.Component> SetComponent { get { return SetOriginalComponent; } }

        private void SetOriginalComponent(UnityEngine.Component comp)
        {
            instance = (UnityEngine.Transform)comp;
        }

        private Transform(UnityEngine.Component comp)
        {
            instance = (UnityEngine.Transform)comp;
        }

        private UnityEngine.Transform instance;

        public UnityEngine.Vector3 GetPosition() { return instance.position; }
        public UnityEngine.Vector3 GetLocalPosition() { return instance.localPosition; }
        public UnityEngine.Quaternion GetRotation() { return instance.rotation; }
        public UnityEngine.Quaternion GetLocalRotation() { return instance.localRotation; }
        public UnityEngine.Vector3 GetEulerAngles() { return instance.eulerAngles; }
        public UnityEngine.Vector3 GetLocalEulerAngles() { return instance.localEulerAngles; }
        public UnityEngine.Vector3 GetLocalScale() { return instance.localScale; }
        public Transform GetParent() { return new Transform(instance.parent); }
        public new Transform GetChild(int num) { return new Transform(instance.GetChild(num)); }
        public new Transform Find(string name) { return new Transform(instance.Find(name)); }

        public new UnityEngine.Vector3 forward { get { return instance.forward; } }
        public new UnityEngine.Vector3 right { get { return instance.right; } }
        public new UnityEngine.Vector3 up { get { return instance.up; } }
        public new string tag { get { return instance.tag; } }


        new public T GetComponent<T>() where T : new()
        {
            //If it is one of the custom game basic component types
            if (typeof(T).BaseType != typeof(GameComponent))
            {
                //Check to make sure the original component version exists
                UnityEngine.Component comp = base.GetComponent(typeof(T).BaseType);

                if (comp != null)
                {
                    //So that we can get original game components and custom components without having to make custom components have IGameComponent stuff
                    return GetNewGameComponent<T>(comp);
                }

                else
                    return default;
            }

            if (typeof(T).Namespace != "Game" && typeof(T).BaseType != typeof(GameComponent))
            {
                UnityEngine.Debug.LogError("GetComponent trying to use class outside of Game namespace");
                return default;
            }

            return base.GetComponent<T>();
        }

        private T GetNewGameComponent<T>(UnityEngine.Component comp) where T : new()
        {
            T gameComp = new T();
            IGameComponent iGame = (IGameComponent)gameComp;
            iGame.SetComponent(comp);
            return gameComp;
        }

        //public UnityEngine.Vector3 localPosition { get; set; }
        //public UnityEngine.Vector3 eulerAngles { get; set; }
        //public UnityEngine.Vector3 localEulerAngles { get; set; }
        //public UnityEngine.Vector3 right { get; set; }
        //public UnityEngine.Vector3 up { get; set; }
        //public UnityEngine.Vector3 forward { get; set; }
        //public UnityEngine.Quaternion rotation { get; set; }
        //public UnityEngine.Vector3 position { get; set; }
        //public UnityEngine.Quaternion localRotation { get; set; }
        //public Transform parent { get; set; }

        public void SetParent(Transform parent)
        {
            int cost = 5;

            if (ManaManager.Instance.ApplyCost(cost))
            {
                instance.parent = parent.instance;
            }

        }

        public void SetParent(Transform parent, bool worldPositionStays)
        {
            int cost = 5;

            if (ManaManager.Instance.ApplyCost(cost))
            {
                instance.SetParent(parent.instance, worldPositionStays);
            }

        }

        private void RevertLocalScale()
        {
            instance.localScale = UnityEngine.Vector3.one;
        }

        //[UnlockStatus("Transform SetLocalScale", false)]
        public void SetLocalScale(UnityEngine.Vector3 scale, bool revert)
        {                                    
            int cost = 10 * (int)scale.magnitude;
            ManaManager manager = ManaManager.Instance;

            if (revert)
            {
                manager.StopCostPerSecond("Transform Local Scale");
                RevertLocalScale();
                return;
            }

            if (manager.CheckCost(cost))
            {
                manager.SetCostPerSecond("Transform Local Scale", cost, RevertLocalScale);
                instance.localScale = scale;
            }
        }
        
        public new UnityEngine.Matrix4x4 worldToLocalMatrix { get { return instance.worldToLocalMatrix; } }
        public new UnityEngine.Matrix4x4 localToWorldMatrix { get { return instance.localToWorldMatrix; } }
        public new Transform root { get { return new Transform(instance.root); } }
        public new int childCount { get { return instance.childCount; } }
        public new UnityEngine.Vector3 lossyScale { get { return instance.lossyScale; } }
        public new int hierarchyCount { get { return instance.hierarchyCount; } }

        //public new void DetachChildren() { return instance.DetachChildren(); }

        public new int GetSiblingIndex() { return instance.GetSiblingIndex(); }
        public new UnityEngine.Vector3 InverseTransformDirection(UnityEngine.Vector3 direction) { return instance.InverseTransformDirection(direction); }
        public new UnityEngine.Vector3 InverseTransformDirection(float x, float y, float z) { return instance.InverseTransformDirection(x, y, z); }
        public new UnityEngine.Vector3 InverseTransformPoint(float x, float y, float z) { return instance.InverseTransformPoint(x, y, z); }
        public new UnityEngine.Vector3 InverseTransformPoint(UnityEngine.Vector3 position) { return instance.InverseTransformPoint(position); }
        public new UnityEngine.Vector3 InverseTransformVector(UnityEngine.Vector3 vector) { return instance.InverseTransformVector(vector); }
        public new UnityEngine.Vector3 InverseTransformVector(float x, float y, float z) { return instance.InverseTransformVector(x, y, z); }
        public bool IsChildOf(Transform parent) { return instance.IsChildOf(parent); }
        public void LookAt(Transform target, UnityEngine.Vector3 worldUp)
        {
            int cost = 5;

            if (ManaManager.Instance.ApplyCost(cost))
            {
                instance.LookAt(target.instance, worldUp);
            }
        }
        public new void LookAt(UnityEngine.Vector3 worldPosition, UnityEngine.Vector3 worldUp)
        {
            int cost = 5;

            if (ManaManager.Instance.ApplyCost(cost))
            {
                instance.LookAt(worldPosition, worldUp);
            }
        }
        public new void LookAt(UnityEngine.Vector3 worldPosition)
        {
            int cost = 5;

            if (ManaManager.Instance.ApplyCost(cost))
            {
                instance.LookAt(worldPosition);
            }
        }
        public void LookAt(Transform target)
        {
            int cost = 5;

            if (ManaManager.Instance.ApplyCost(cost))
            {
                instance.LookAt(target.instance);
            }
        }

        public new void Rotate(float xAngle, float yAngle, float zAngle)
        {
            int cost = (int)new UnityEngine.Vector3(xAngle, yAngle, zAngle).magnitude;

            if (ManaManager.Instance.ApplyCost(cost))
            {
                instance.Rotate(xAngle, yAngle, zAngle);
            }
        }
        public new void Rotate(UnityEngine.Vector3 eulers, UnityEngine.Space relativeTo)
        {
            int cost = (int)eulers.magnitude;

            if (ManaManager.Instance.ApplyCost(cost))
            {
                instance.Rotate(eulers, relativeTo);
            }
        }
        public new void Rotate(UnityEngine.Vector3 eulers)
        {
            int cost = (int)eulers.magnitude;

            if (ManaManager.Instance.ApplyCost(cost))
            {
                instance.Rotate(eulers);
            }
        }

        public new void Rotate(float xAngle, float yAngle, float zAngle, UnityEngine.Space relativeTo)
        {
            int cost = (int)new UnityEngine.Vector3(xAngle, yAngle, zAngle).magnitude;

            if (ManaManager.Instance.ApplyCost(cost))
            {
                instance.Rotate(xAngle, yAngle, zAngle, relativeTo);
            }
        }
        public new void Rotate(UnityEngine.Vector3 axis, float angle, UnityEngine.Space relativeTo)
        {
            int cost = (int)UnityEngine.Mathf.Abs(angle);

            if (ManaManager.Instance.ApplyCost(cost))
            {
                instance.Rotate(axis, angle, relativeTo);
            }

        }

        public new void Rotate(UnityEngine.Vector3 axis, float angle)
        {
            int cost = (int)UnityEngine.Mathf.Abs(angle);

            if (ManaManager.Instance.ApplyCost(cost))
            {
                instance.Rotate(axis, angle);
            }
        }

        public new void RotateAround(UnityEngine.Vector3 point, UnityEngine.Vector3 axis, float angle)
        {
            int cost = (int)(point - instance.position).magnitude;

            if (ManaManager.Instance.ApplyCost(cost))
            {
                instance.RotateAround(point, axis, angle);
            }
        }

        public new void SetPositionAndRotation(UnityEngine.Vector3 position, UnityEngine.Quaternion rotation)
        {
            int cost = (int)((position - instance.position).magnitude * 2.0f);
        }

        public new UnityEngine.Vector3 TransformDirection(float x, float y, float z) { return instance.TransformDirection(x, y, z); }
        public new UnityEngine.Vector3 TransformDirection(UnityEngine.Vector3 direction) { return instance.TransformDirection(direction); }
        public new UnityEngine.Vector3 TransformPoint(float x, float y, float z) { return instance.TransformPoint(x, y, z); }
        public new UnityEngine.Vector3 TransformPoint(UnityEngine.Vector3 position) { return instance.TransformPoint(position); }
        public new UnityEngine.Vector3 TransformVector(float x, float y, float z) { return instance.TransformVector(x, y, z); }
        public new UnityEngine.Vector3 TransformVector(UnityEngine.Vector3 vector) { return instance.TransformVector(vector); }

        public new void Translate(float x, float y, float z)
        {
            int cost = 10 * (int)(new UnityEngine.Vector3(x, y, z) - instance.position).magnitude;

            if (ManaManager.Instance.ApplyCost(cost))
            {
                instance.Translate(x, y, z);
            }
        }
        public new void Translate(float x, float y, float z, UnityEngine.Space relativeTo)
        {
            int cost = 10 * (int)(new UnityEngine.Vector3(x, y, z) - instance.position).magnitude;

            if (ManaManager.Instance.ApplyCost(cost))
            {
                instance.Translate(x, y, z, relativeTo);
            }
        }
        public new void Translate(UnityEngine.Vector3 translation)
        {
            int cost = 10 * (int)(translation - instance.position).magnitude;

            if (ManaManager.Instance.ApplyCost(cost))
            {
                instance.Translate(translation);
            }
        }
        public new void Translate(UnityEngine.Vector3 translation, UnityEngine.Space relativeTo)
        {
            int cost = 10 * (int)(translation - instance.position).magnitude;

            if (ManaManager.Instance.ApplyCost(cost))
            {
                instance.Translate(translation, relativeTo);
            }
        }
        public void Translate(float x, float y, float z, Transform relativeTo)
        {
            int cost = 10 * (int)(new UnityEngine.Vector3(x, y, z) - instance.position).magnitude;

            if (ManaManager.Instance.ApplyCost(cost))
            {
                instance.Translate(x, y, z, relativeTo);
            }
        }
        public void Translate(UnityEngine.Vector3 translation, Transform relativeTo)
        {
            int cost = 10 * (int)(translation - instance.position).magnitude;

            if (ManaManager.Instance.ApplyCost(cost))
            {
                instance.Translate(translation, relativeTo);
            }
        }

    }
}
