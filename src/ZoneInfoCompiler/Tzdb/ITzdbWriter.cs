// Copyright 2012 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace NodaTime.ZoneInfoCompiler.Tzdb
{
    interface ITzdbWriter
    {
        void Write(TzdbDatabase tzdb, WindowsMapping mapping);
    }
}
