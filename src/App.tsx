import { useState, useEffect } from "react";
import { Toaster } from "@/components/ui/toaster";
import { Toaster as Sonner } from "@/components/ui/sonner";
import { TooltipProvider } from "@/components/ui/tooltip";
import { ThemeProvider } from "@/components/ThemeProvider";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import Index from "./pages/Index";
import Setup from "./pages/Setup";
import History from "./pages/History";
import Comparison from "./pages/Comparison";
import Monitor from "./pages/Monitor";
import RecordDetail from "./pages/RecordDetail";
import Admin from "./pages/Admin";
import AccountSettings from "./pages/AccountSettings";
import TransferLogDetail from "./pages/TransferLogDetail";
import Friends from "./pages/Friends";
import Chat from "./pages/Chat";
import Feed from "./pages/Feed";
import AiCoach from "./pages/AiCoach";
import NotFound from "./pages/NotFound";

const App = () => {
  const [queryClient] = useState(() => new QueryClient());
  const [isSetupComplete, setIsSetupComplete] = useState(false);

  useEffect(() => {
    const setupComplete = localStorage.getItem("setup_completed");
    setIsSetupComplete(setupComplete === "true");
  }, []);
  
  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider>
        <TooltipProvider>
          <Toaster />
          <Sonner />
          <BrowserRouter>
            <Routes>
              <Route path="/setup" element={<Setup />} />
              <Route path="/" element={isSetupComplete ? <Index /> : <Navigate to="/setup" replace />} />
              <Route path="/history" element={<History />} />
              <Route path="/record/:id" element={<RecordDetail />} />
              <Route path="/comparison" element={<Comparison />} />
              <Route path="/friends" element={<Friends />} />
              <Route path="/chat" element={<Chat />} />
              <Route path="/feed" element={<Feed />} />
              <Route path="/ai-coach" element={<AiCoach />} />
              <Route path="/monitor" element={<Monitor />} />
              <Route path="/monitor/log/:id" element={<TransferLogDetail />} />
              <Route path="/admin" element={<Admin />} />
              <Route path="/account-settings" element={<AccountSettings />} />
              <Route path="*" element={<NotFound />} />
            </Routes>
          </BrowserRouter>
        </TooltipProvider>
      </ThemeProvider>
    </QueryClientProvider>
  );
};

export default App;
