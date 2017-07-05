// Copyright (c) 2014-2015 Wolfgang Borgsmüller
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// 1. Redistributions of source code must retain the above copyright 
//    notice, this list of conditions and the following disclaimer.
// 
// 2. Redistributions in binary form must reproduce the above copyright 
//    notice, this list of conditions and the following disclaimer in the 
//    documentation and/or other materials provided with the distribution.
// 
// 3. Neither the name of the copyright holder nor the names of its 
//    contributors may be used to endorse or promote products derived 
//    from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS 
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT 
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS 
// FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE 
// COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, 
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, 
// BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS 
// OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND 
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR 
// TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE 
// USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.



using Chromium;
using Chromium.Remote;
using System;
using System.Collections.Generic;

namespace NetDimension.NanUI.ChromiumCore
{
	public class RenderProcess
	{

		internal static int RenderProcessMain()
		{
			try
			{
				var rp = new RenderProcess();
				HtmlUILauncher.RaiseRemoteProcessCreated(rp.processHandler);
				return rp.RemoteMain();
			}
			catch (CfxRemotingException)
			{
				return -1;
			}
		}

		private readonly CfrApp app;
		private readonly RenderProcessHandler processHandler;

		private List<WeakReference> browserReferences = new List<WeakReference>();

		internal int RemoteProcessId { get; private set; }

		private RenderProcess()
		{
			RemoteProcessId = CfxRemoteCallContext.CurrentContext.ProcessId;
			app = new CfrApp();
			processHandler = new RenderProcessHandler(this);
			app.GetRenderProcessHandler += (s, e) => e.SetReturnValue(processHandler);
		}

		internal void AddBrowserReference(IChromiumWebBrowser browser)
		{
			for (int i = 0; i < browserReferences.Count; ++i)
			{
				if (browserReferences[i].Target == null)
				{
					browserReferences[i] = new WeakReference(browser);
					return;
				}
			}
			browserReferences.Add(new WeakReference(browser));
		}

		private int RemoteMain()
		{
			try
			{
				var retval = CfrRuntime.ExecuteProcess(app);
				return retval;
			}
			finally
			{
				foreach (var br in browserReferences)
				{
					var b = (IChromiumWebBrowser)br.Target;
					b?.RemoteProcessExited(this);
				}
			}
		}

	}
}
