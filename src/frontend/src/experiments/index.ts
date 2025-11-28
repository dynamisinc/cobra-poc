/**
 * Experiments Module
 *
 * Config-based UX variant selection for testing different
 * checklist experiences with a small user base.
 */

export {
  type ChecklistVariant,
  type VariantInfo,
  checklistVariants,
  getCurrentVariant,
  setVariant,
  isValidVariant,
  getVariantInfo,
  VARIANT_STORAGE_KEY,
  VARIANT_URL_PARAM,
} from './experimentConfig';

export { useChecklistVariant } from './useChecklistVariant';
