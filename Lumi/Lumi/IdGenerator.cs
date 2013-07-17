using System.Diagnostics;
using System;
namespace Lumi
{
    public static class IdGenerator
    {
        static int Id = 0;
        static bool firstTime = true;

        public static int CreateId()
        {
            Id = Id + 1;
            return Id;
        }

        public static int MoveLeft { get; private set; }
        public static int MoveRight { get; private set; }
        public static int MoveUp { get; private set; }
        public static int MoveDown { get; private set; }
        public static int Jump { get; private set; }
        public static int LookUp { get; private set; }

        public static int OffsetLeft { get; private set; }
        public static int OffsetRight { get; private set; }
        public static int OffsetUp { get; private set; }
        public static int OffsetDown { get; private set; }

        public static int CaptureMouseLeft { get; private set; }
        public static int CaptureMouseRight { get; private set; }

        public static int Shoot { get; private set; }

        public static void Initialize()
        {
            Debug.WriteLineIf(firstTime, "IdGenerator was initialized more than once!");
            if (!firstTime) return;

            firstTime = false;

            MoveLeft = CreateId();
            MoveRight = CreateId();
            MoveUp = CreateId();
            MoveDown = CreateId();
            Jump = CreateId();
            LookUp = CreateId();
            OffsetLeft = CreateId();
            OffsetRight = CreateId();
            OffsetUp = CreateId();
            OffsetDown = CreateId();
            CaptureMouseLeft = CreateId();
            CaptureMouseRight = CreateId();
            Shoot = CreateId();
        }
    }
}