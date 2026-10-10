import { useAuthRedirect } from "../../hooks/useAuthRedirect";
import { useUnauthorizedInvalidation } from "../../hooks/useUnauthorizedInvalidation";

type Props = {
  children: React.ReactNode;
};

/**
 * Gates rendering of its children until auth state is resolved,
 * and wires up global unauthorized handling.
 *
 * While auth is loading, nothing is rendered.
 *
 * @param props - Component props.
 * @param props.children - The subtree to render once auth is resolved.
 * @returns The children when auth is resolved, otherwise null.
 */
export function AuthGuard({ children }: Props) {
  useUnauthorizedInvalidation();
  const { isPending } = useAuthRedirect(); 
  if (isPending) return null;

  return <>{children}</>;
}
