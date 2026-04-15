# Orchestration: Exception Handling Decision Proposed

**Agent:** Ripley  
**Timestamp:** 2026-04-14T18:26:31Z  
**Event:** Decision written to inbox  

## Summary

Ripley completed a deep audit of exception handling across the MoreSpeakers codebase:
- Counted **69 catch blocks**, all bare `catch(Exception)`
- Identified **3 conflicting failure patterns**:
  1. Swallow-and-return-sentinel (most common)
  2. Catch-log-throw-ApplicationException (DataStore SaveAsync)
  3. Catch-everything-at-Web-layer (~29 handlers)

**Recommendation:** Adopt `Result<T>` pattern over rethrow-with-context.

**Rationale:**
- Rethrow-with-context only fixes pattern #2
- Result<T> handles all three patterns uniformly
- IdentityResult already proves pattern works in codebase
- Proposed incremental rollout: Foundation → PoC (Expertise) → Extend → Cleanup

**Status:** Awaiting team consensus. Written to inbox for merge.

**Affects:** All agents, especially api-agent (Web layer), test-agent (DataStore tests).

---

## Next Steps

- Squad votes on Result<T> adoption
- If approved, Phase 1 (Domain) begins
- Unblocks XSS security fixes and DataStore test harness design
