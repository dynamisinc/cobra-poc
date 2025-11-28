import { apiClient } from './api';
import type {
  ItemLibraryEntry,
  CreateItemLibraryEntryRequest,
  UpdateItemLibraryEntryRequest,
} from '../types';

/**
 * Item Library Service
 * Handles all API calls for managing item library entries
 */

export const itemLibraryService = {
  /**
   * Get all library items with optional filtering and sorting
   */
  async getLibraryItems(params?: {
    category?: string;
    itemType?: string;
    searchText?: string;
    sortBy?: 'recent' | 'popular' | 'alphabetical';
  }): Promise<ItemLibraryEntry[]> {
    const response = await apiClient.get<ItemLibraryEntry[]>('/itemlibrary', {
      params,
    });
    return response.data;
  },

  /**
   * Get a specific library item by ID
   */
  async getLibraryItemById(id: string): Promise<ItemLibraryEntry> {
    const response = await apiClient.get<ItemLibraryEntry>(`/itemlibrary/${id}`);
    return response.data;
  },

  /**
   * Create a new library item
   */
  async createLibraryItem(request: CreateItemLibraryEntryRequest): Promise<ItemLibraryEntry> {
    const response = await apiClient.post<ItemLibraryEntry>('/itemlibrary', request);
    return response.data;
  },

  /**
   * Update an existing library item
   */
  async updateLibraryItem(
    id: string,
    request: UpdateItemLibraryEntryRequest
  ): Promise<ItemLibraryEntry> {
    const response = await apiClient.put<ItemLibraryEntry>(`/itemlibrary/${id}`, request);
    return response.data;
  },

  /**
   * Increment usage count for a library item (when added to template)
   */
  async incrementUsageCount(id: string): Promise<void> {
    await apiClient.post(`/itemlibrary/${id}/increment-usage`);
  },

  /**
   * Archive a library item (soft delete)
   */
  async archiveLibraryItem(id: string): Promise<void> {
    await apiClient.delete(`/itemlibrary/${id}`);
  },

  /**
   * Restore an archived library item
   */
  async restoreLibraryItem(id: string): Promise<void> {
    await apiClient.post(`/itemlibrary/${id}/restore`);
  },

  /**
   * Permanently delete a library item (admin only)
   */
  async deleteLibraryItem(id: string): Promise<void> {
    await apiClient.delete(`/itemlibrary/${id}/permanent`);
  },
};
