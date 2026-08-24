import { create } from "zustand";
import { getWallet, type WalletSummaryResult } from "../../features/auth/api";

interface WalletState {
  summary: WalletSummaryResult | null;
  loading: boolean;
  load: () => Promise<void>;
  setSummary: (summary: WalletSummaryResult | null) => void;
}

export const useWalletStore = create<WalletState>((set) => ({
  summary: null,
  loading: true,
  load: async () => {
    set({ loading: true });

    try {
      const result = await getWallet();
      if (result.success && result.data) {
        set({ summary: result.data, loading: false });
        return;
      }

      set({ summary: null, loading: false });
    } catch {
      set({ summary: null, loading: false });
    }
  },
  setSummary: (summary) => set({ summary }),
}));
