import { useState, useEffect } from "react";
import { Toaster } from "@/components/ui/toaster";
import { Toaster as Sonner } from "@/components/ui/sonner";
import { TooltipProvider } from "@/components/ui/tooltip";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import Index from "./pages/Index";
import History from "./pages/History";
import Comparison from "./pages/Comparison";
import Monitor from "./pages/Monitor";
import Admin from "./pages/Admin";
import Setup from "./pages/Setup";
import RecordDetail from "./pages/RecordDetail";
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
            <Route path="/monitor" element={<Monitor />} />
            <Route path="/admin" element={<Admin />} />
            {/* ADD ALL CUSTOM ROUTES ABOVE THE CATCH-ALL "*" ROUTE */}
            <Route path="*" element={<NotFound />} />
          </Routes>
        </BrowserRouter>
      </TooltipProvider>
    </QueryClientProvider>
  );
};

export default App;
