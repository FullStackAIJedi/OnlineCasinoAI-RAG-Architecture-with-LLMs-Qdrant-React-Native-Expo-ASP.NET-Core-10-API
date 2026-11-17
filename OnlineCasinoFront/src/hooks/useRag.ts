import { useMutation } from "@tanstack/react-query";
import { ragApi } from "../api/rag";
import type { AskResponse } from "../api/rag";

/**
 * Mutation hook for asking a question to Casino.RagService.
 *
 * Does not cache responses — each question is unique and depends on the
 * current conversation context in the UI.
 */
export const useAskQuestion = () =>
  useMutation<AskResponse, Error, string>({
    mutationFn: (question: string) => ragApi.ask(question),
  });
