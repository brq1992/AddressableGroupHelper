namespace AddressableAssetTool.Graph
{
    internal class EdgeUserData
    {
        private string assetPath;
        private string dependencyString;

        public EdgeUserData(string assetPath, string dependencyString)
        {
            this.assetPath = assetPath;
            this.dependencyString = dependencyString;
        }

        public string ParentPath { get { return assetPath; } }
        public string Dependence { get { return dependencyString; } }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            EdgeUserData other = (EdgeUserData)obj;
            return ParentPath == other.ParentPath && Dependence == other.Dependence;
        }


        public override int GetHashCode()
        {
            unchecked 
            {
                int hash = 17;
                hash = hash * 23 + (ParentPath != null ? ParentPath.GetHashCode() : 0);
                hash = hash * 23 + (Dependence != null ? Dependence.GetHashCode() : 0);
                return hash;
            }
        }
    }
}