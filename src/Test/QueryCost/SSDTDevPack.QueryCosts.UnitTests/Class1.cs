using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace SSDTDevPack.QueryCosts.UnitTests
{
    [TestFixture]
    public class PlanParserTests
    {
        [Test]
        public void Retrieves_Cost_From_Simple_Statement()
        {
            var xml = simplePlan;

            var gateway = new Mock<QueryCostDataGateway>();
            gateway.Setup(p => p.GetPlanForQuery(It.IsAny<string>())).Returns(xml);

            var parser = new PlanParser(gateway.Object);
            
            var statements = parser.GetStatements("blah");
            Assert.AreEqual(1, statements.Count);
            Assert.AreEqual(CostBand.Medium, statements.FirstOrDefault().Band);


        }

        [Test]
        public void Retrieves_Cost_From_Multiple_Simple_Statement()
        {
            var xml = simplePlan;

            var gateway = new Mock<QueryCostDataGateway>();
            gateway.Setup(p => p.GetPlanForQuery(It.IsAny<string>())).Returns(complexPlan);

            var parser = new PlanParser(gateway.Object);

            var statements = parser.GetStatements("blah");
            Assert.AreEqual(4, statements.Count);
            Assert.AreEqual(CostBand.High, statements[2].Band);


        }




#region plans

        private const string complexPlan = @"<?xml version=""1.0"" encoding=""utf-16""?>
<ShowPlanXML xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Version=""1.1"" Build=""10.50.1600.1"" xmlns=""http://schemas.microsoft.com/sqlserver/2004/07/showplan"">
  <BatchSequence>
    <Batch>
      <Statements>
        <StmtSimple StatementCompId=""1"" StatementId=""1"" StatementText=""read_it"" StatementType=""EXECUTE PROC"" />
      </Statements>
      <Statements>
        <StmtSimple>
          <StoredProc ProcName=""read_it"">
            <Statements>
              <StmtSimple StatementCompId=""3"" StatementEstRows=""1"" StatementId=""2"" StatementOptmLevel=""FULL"" StatementOptmEarlyAbortReason=""GoodEnoughPlanFound"" StatementSubTreeCost=""1.0297978"" StatementText=""CREATE procedure read_it(@a int)&#xD;&#xA;as&#xD;&#xA;&#xD;&#xA;	insert into table_one(a, b)&#xD;&#xA;	select a, 'dddddd' from table_one where a in (select a from table_one where a &lt;&gt; @a);&#xD;&#xA;&#xD;&#xA;"" StatementType=""INSERT"" QueryHash=""0xBFB553CC06539DAB"" QueryPlanHash=""0xBE52BBC43DCF5F5E"">
                <StatementSetOptions ANSI_NULLS=""true"" ANSI_PADDING=""true"" ANSI_WARNINGS=""true"" ARITHABORT=""true"" CONCAT_NULL_YIELDS_NULL=""true"" NUMERIC_ROUNDABORT=""false"" QUOTED_IDENTIFIER=""true"" />
                <QueryPlan CachedPlanSize=""24"" CompileTime=""12"" CompileCPU=""8"" CompileMemory=""344"">
                  <RelOp AvgRowSize=""9"" EstimateCPU=""1E-06"" EstimateIO=""0.01"" EstimateRebinds=""0"" EstimateRewinds=""0"" EstimateRows=""1"" LogicalOp=""Insert"" NodeId=""0"" Parallel=""false"" PhysicalOp=""Table Insert"" EstimatedTotalSubtreeCost=""0.0297978"">
                    <OutputList />
                    <Update DMLRequestSort=""false"">
                      <Object Database=""[DacPac]"" Schema=""[dbo]"" Table=""[table_one]"" IndexKind=""Heap"" />
                      <SetPredicate>
                        <ScalarOperator ScalarString=""[DacPac].[dbo].[table_one].[a] = [DacPac].[dbo].[table_one].[a],[DacPac].[dbo].[table_one].[b] = [Expr1012]"">
                          <ScalarExpressionList>
                            <ScalarOperator>
                              <MultipleAssign>
                                <Assign>
                                  <ColumnReference Database=""[DacPac]"" Schema=""[dbo]"" Table=""[table_one]"" Column=""a"" />
                                  <ScalarOperator>
                                    <Identifier>
                                      <ColumnReference Database=""[DacPac]"" Schema=""[dbo]"" Table=""[table_one]"" Column=""a"" />
                                    </Identifier>
                                  </ScalarOperator>
                                </Assign>
                                <Assign>
                                  <ColumnReference Database=""[DacPac]"" Schema=""[dbo]"" Table=""[table_one]"" Column=""b"" />
                                  <ScalarOperator>
                                    <Identifier>
                                      <ColumnReference Column=""Expr1012"" />
                                    </Identifier>
                                  </ScalarOperator>
                                </Assign>
                              </MultipleAssign>
                            </ScalarOperator>
                          </ScalarExpressionList>
                        </ScalarOperator>
                      </SetPredicate>
                      <RelOp AvgRowSize=""4039"" EstimateCPU=""1E-07"" EstimateIO=""0"" EstimateRebinds=""0"" EstimateRewinds=""0"" EstimateRows=""1"" LogicalOp=""Compute Scalar"" NodeId=""1"" Parallel=""false"" PhysicalOp=""Compute Scalar"" EstimatedTotalSubtreeCost=""0.0197968"">
                        <OutputList>
                          <ColumnReference Database=""[DacPac]"" Schema=""[dbo]"" Table=""[table_one]"" Column=""a"" />
                          <ColumnReference Column=""Expr1012"" />
                        </OutputList>
                        <ComputeScalar>
                          <DefinedValues>
                            <DefinedValue>
                              <ColumnReference Column=""Expr1012"" />
                              <ScalarOperator ScalarString=""CONVERT_IMPLICIT(varchar(max),'dddddd',0)"">
                                <Identifier>
                                  <ColumnReference Column=""ConstExpr1013"">
                                    <ScalarOperator>
                                      <Convert DataType=""varchar(max)"" Length=""2147483647"" Style=""0"" Implicit=""true"">
                                        <ScalarOperator>
                                          <Const ConstValue=""'dddddd'"" />
                                        </ScalarOperator>
                                      </Convert>
                                    </ScalarOperator>
                                  </ColumnReference>
                                </Identifier>
                              </ScalarOperator>
                            </DefinedValue>
                          </DefinedValues>
                          <RelOp AvgRowSize=""11"" EstimateCPU=""0.00010046"" EstimateIO=""0.013125"" EstimateRebinds=""0"" EstimateRewinds=""0"" EstimateRows=""1"" LogicalOp=""Eager Spool"" NodeId=""2"" Parallel=""false"" PhysicalOp=""Table Spool"" EstimatedTotalSubtreeCost=""0.0197967"">
                            <OutputList>
                              <ColumnReference Database=""[DacPac]"" Schema=""[dbo]"" Table=""[table_one]"" Column=""a"" />
                            </OutputList>
                            <Spool>
                              <RelOp AvgRowSize=""11"" EstimateCPU=""1E-07"" EstimateIO=""0"" EstimateRebinds=""0"" EstimateRewinds=""0"" EstimateRows=""1"" LogicalOp=""Top"" NodeId=""3"" Parallel=""false"" PhysicalOp=""Top"" EstimatedTotalSubtreeCost=""0.00657126"">
                                <OutputList>
                                  <ColumnReference Database=""[DacPac]"" Schema=""[dbo]"" Table=""[table_one]"" Column=""a"" />
                                </OutputList>
                                <Top RowCount=""true"" IsPercent=""false"" WithTies=""false"">
                                  <TopExpression>
                                    <ScalarOperator ScalarString=""(0)"">
                                      <Const ConstValue=""(0)"" />
                                    </ScalarOperator>
                                  </TopExpression>
                                  <RelOp AvgRowSize=""11"" EstimateCPU=""4.18E-06"" EstimateIO=""0"" EstimateRebinds=""0"" EstimateRewinds=""0"" EstimateRows=""1"" LogicalOp=""Left Semi Join"" NodeId=""4"" Parallel=""false"" PhysicalOp=""Nested Loops"" EstimatedTotalSubtreeCost=""0.00657116"">
                                    <OutputList>
                                      <ColumnReference Database=""[DacPac]"" Schema=""[dbo]"" Table=""[table_one]"" Column=""a"" />
                                    </OutputList>
                                    <NestedLoops Optimized=""false"">
                                      <Predicate>
                                        <ScalarOperator ScalarString=""[DacPac].[dbo].[table_one].[a]=[DacPac].[dbo].[table_one].[a]"">
                                          <Compare CompareOp=""EQ"">
                                            <ScalarOperator>
                                              <Identifier>
                                                <ColumnReference Database=""[DacPac]"" Schema=""[dbo]"" Table=""[table_one]"" Column=""a"" />
                                              </Identifier>
                                            </ScalarOperator>
                                            <ScalarOperator>
                                              <Identifier>
                                                <ColumnReference Database=""[DacPac]"" Schema=""[dbo]"" Table=""[table_one]"" Column=""a"" />
                                              </Identifier>
                                            </ScalarOperator>
                                          </Compare>
                                        </ScalarOperator>
                                      </Predicate>
                                      <RelOp AvgRowSize=""19"" EstimateCPU=""0.0001581"" EstimateIO=""0.003125"" EstimateRebinds=""0"" EstimateRewinds=""0"" EstimateRows=""1"" LogicalOp=""Table Scan"" NodeId=""5"" Parallel=""false"" PhysicalOp=""Table Scan"" EstimatedTotalSubtreeCost=""0.0032831"" TableCardinality=""0"">
                                        <OutputList>
                                          <ColumnReference Database=""[DacPac]"" Schema=""[dbo]"" Table=""[table_one]"" Column=""a"" />
                                        </OutputList>
                                        <TableScan Ordered=""false"" ForcedIndex=""false"" NoExpandHint=""false"">
                                          <DefinedValues>
                                            <DefinedValue>
                                              <ColumnReference Database=""[DacPac]"" Schema=""[dbo]"" Table=""[table_one]"" Column=""a"" />
                                            </DefinedValue>
                                          </DefinedValues>
                                          <Object Database=""[DacPac]"" Schema=""[dbo]"" Table=""[table_one]"" TableReferenceId=""1"" IndexKind=""Heap"" />
                                        </TableScan>
                                      </RelOp>
                                      <RelOp AvgRowSize=""11"" EstimateCPU=""0.0001581"" EstimateIO=""0.003125"" EstimateRebinds=""0"" EstimateRewinds=""0"" EstimateRows=""1"" LogicalOp=""Table Scan"" NodeId=""6"" Parallel=""false"" PhysicalOp=""Table Scan"" EstimatedTotalSubtreeCost=""0.0032831"" TableCardinality=""0"">
                                        <OutputList>
                                          <ColumnReference Database=""[DacPac]"" Schema=""[dbo]"" Table=""[table_one]"" Column=""a"" />
                                        </OutputList>
                                        <TableScan Ordered=""false"" ForcedIndex=""false"" NoExpandHint=""false"">
                                          <DefinedValues>
                                            <DefinedValue>
                                              <ColumnReference Database=""[DacPac]"" Schema=""[dbo]"" Table=""[table_one]"" Column=""a"" />
                                            </DefinedValue>
                                          </DefinedValues>
                                          <Object Database=""[DacPac]"" Schema=""[dbo]"" Table=""[table_one]"" TableReferenceId=""2"" IndexKind=""Heap"" />
                                          <Predicate>
                                            <ScalarOperator ScalarString=""[DacPac].[dbo].[table_one].[a]&lt;&gt;[@a]"">
                                              <Compare CompareOp=""NE"">
                                                <ScalarOperator>
                                                  <Identifier>
                                                    <ColumnReference Database=""[DacPac]"" Schema=""[dbo]"" Table=""[table_one]"" Column=""a"" />
                                                  </Identifier>
                                                </ScalarOperator>
                                                <ScalarOperator>
                                                  <Identifier>
                                                    <ColumnReference Column=""@a"" />
                                                  </Identifier>
                                                </ScalarOperator>
                                              </Compare>
                                            </ScalarOperator>
                                          </Predicate>
                                        </TableScan>
                                      </RelOp>
                                    </NestedLoops>
                                  </RelOp>
                                </Top>
                              </RelOp>
                            </Spool>
                          </RelOp>
                        </ComputeScalar>
                      </RelOp>
                    </Update>
                  </RelOp>
                </QueryPlan>
              </StmtSimple>
              <StmtCond StatementCompId=""4"" StatementId=""3"" StatementText=""	if(@a = 100110)&#xD;&#xA;	"" StatementType=""COND"">
                <Condition />
                <Then>
                  <Statements>
                    <StmtSimple StatementCompId=""5"" StatementId=""4"" StatementText=""	exec read_it 988&#xD;"" StatementType=""EXECUTE PROC"" />
                  </Statements>
                </Then>
              </StmtCond>
            </Statements>
          </StoredProc>
        </StmtSimple>
      </Statements>
    </Batch>
  </BatchSequence>
</ShowPlanXML>";

        private const string simplePlan = @"<?xml version=""1.0"" encoding=""utf-16""?>
<ShowPlanXML xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Version=""1.1"" Build=""10.50.1600.1"" xmlns=""http://schemas.microsoft.com/sqlserver/2004/07/showplan"">
  <BatchSequence>
    <Batch>
      <Statements>
        <StmtSimple StatementCompId=""1"" StatementEstRows=""1"" StatementId=""1"" StatementOptmLevel=""FULL"" StatementOptmEarlyAbortReason=""GoodEnoughPlanFound"" StatementSubTreeCost=""0.9297978"" StatementText=""&#xD;&#xA;	insert into table_one(a, b)&#xD;&#xA;	select a, 'dddddd' from table_one where a in (select a from table_one where a &lt;&gt; 32);"" StatementType=""INSERT"" QueryHash=""0xBFB553CC06539DAB"" QueryPlanHash=""0xBE52BBC43DCF5F5E"">
          <StatementSetOptions ANSI_NULLS=""true"" ANSI_PADDING=""true"" ANSI_WARNINGS=""true"" ARITHABORT=""true"" CONCAT_NULL_YIELDS_NULL=""true"" NUMERIC_ROUNDABORT=""false"" QUOTED_IDENTIFIER=""true"" />
          <QueryPlan CachedPlanSize=""24"" CompileTime=""4"" CompileCPU=""4"" CompileMemory=""336"">
            <RelOp AvgRowSize=""9"" EstimateCPU=""1E-06"" EstimateIO=""0.01"" EstimateRebinds=""0"" EstimateRewinds=""0"" EstimateRows=""1"" LogicalOp=""Insert"" NodeId=""0"" Parallel=""false"" PhysicalOp=""Table Insert"" EstimatedTotalSubtreeCost=""0.0297978"">
              <OutputList />
              <Update DMLRequestSort=""false"">
                <Object Database=""[DacPac]"" Schema=""[dbo]"" Table=""[table_one]"" IndexKind=""Heap"" />
                <SetPredicate>
                  <ScalarOperator ScalarString=""[DacPac].[dbo].[table_one].[a] = [DacPac].[dbo].[table_one].[a],[DacPac].[dbo].[table_one].[b] = [Expr1012]"">
                    <ScalarExpressionList>
                      <ScalarOperator>
                        <MultipleAssign>
                          <Assign>
                            <ColumnReference Database=""[DacPac]"" Schema=""[dbo]"" Table=""[table_one]"" Column=""a"" />
                            <ScalarOperator>
                              <Identifier>
                                <ColumnReference Database=""[DacPac]"" Schema=""[dbo]"" Table=""[table_one]"" Column=""a"" />
                              </Identifier>
                            </ScalarOperator>
                          </Assign>
                          <Assign>
                            <ColumnReference Database=""[DacPac]"" Schema=""[dbo]"" Table=""[table_one]"" Column=""b"" />
                            <ScalarOperator>
                              <Identifier>
                                <ColumnReference Column=""Expr1012"" />
                              </Identifier>
                            </ScalarOperator>
                          </Assign>
                        </MultipleAssign>
                      </ScalarOperator>
                    </ScalarExpressionList>
                  </ScalarOperator>
                </SetPredicate>
                <RelOp AvgRowSize=""4039"" EstimateCPU=""1E-07"" EstimateIO=""0"" EstimateRebinds=""0"" EstimateRewinds=""0"" EstimateRows=""1"" LogicalOp=""Compute Scalar"" NodeId=""1"" Parallel=""false"" PhysicalOp=""Compute Scalar"" EstimatedTotalSubtreeCost=""0.0197968"">
                  <OutputList>
                    <ColumnReference Database=""[DacPac]"" Schema=""[dbo]"" Table=""[table_one]"" Column=""a"" />
                    <ColumnReference Column=""Expr1012"" />
                  </OutputList>
                  <ComputeScalar>
                    <DefinedValues>
                      <DefinedValue>
                        <ColumnReference Column=""Expr1012"" />
                        <ScalarOperator ScalarString=""CONVERT_IMPLICIT(varchar(max),'dddddd',0)"">
                          <Identifier>
                            <ColumnReference Column=""ConstExpr1013"">
                              <ScalarOperator>
                                <Convert DataType=""varchar(max)"" Length=""2147483647"" Style=""0"" Implicit=""true"">
                                  <ScalarOperator>
                                    <Const ConstValue=""'dddddd'"" />
                                  </ScalarOperator>
                                </Convert>
                              </ScalarOperator>
                            </ColumnReference>
                          </Identifier>
                        </ScalarOperator>
                      </DefinedValue>
                    </DefinedValues>
                    <RelOp AvgRowSize=""11"" EstimateCPU=""0.00010046"" EstimateIO=""0.013125"" EstimateRebinds=""0"" EstimateRewinds=""0"" EstimateRows=""1"" LogicalOp=""Eager Spool"" NodeId=""2"" Parallel=""false"" PhysicalOp=""Table Spool"" EstimatedTotalSubtreeCost=""0.0197967"">
                      <OutputList>
                        <ColumnReference Database=""[DacPac]"" Schema=""[dbo]"" Table=""[table_one]"" Column=""a"" />
                      </OutputList>
                      <Spool>
                        <RelOp AvgRowSize=""11"" EstimateCPU=""1E-07"" EstimateIO=""0"" EstimateRebinds=""0"" EstimateRewinds=""0"" EstimateRows=""1"" LogicalOp=""Top"" NodeId=""3"" Parallel=""false"" PhysicalOp=""Top"" EstimatedTotalSubtreeCost=""0.00657126"">
                          <OutputList>
                            <ColumnReference Database=""[DacPac]"" Schema=""[dbo]"" Table=""[table_one]"" Column=""a"" />
                          </OutputList>
                          <Top RowCount=""true"" IsPercent=""false"" WithTies=""false"">
                            <TopExpression>
                              <ScalarOperator ScalarString=""(0)"">
                                <Const ConstValue=""(0)"" />
                              </ScalarOperator>
                            </TopExpression>
                            <RelOp AvgRowSize=""11"" EstimateCPU=""4.18E-06"" EstimateIO=""0"" EstimateRebinds=""0"" EstimateRewinds=""0"" EstimateRows=""1"" LogicalOp=""Left Semi Join"" NodeId=""4"" Parallel=""false"" PhysicalOp=""Nested Loops"" EstimatedTotalSubtreeCost=""0.00657116"">
                              <OutputList>
                                <ColumnReference Database=""[DacPac]"" Schema=""[dbo]"" Table=""[table_one]"" Column=""a"" />
                              </OutputList>
                              <NestedLoops Optimized=""false"">
                                <Predicate>
                                  <ScalarOperator ScalarString=""[DacPac].[dbo].[table_one].[a]=[DacPac].[dbo].[table_one].[a]"">
                                    <Compare CompareOp=""EQ"">
                                      <ScalarOperator>
                                        <Identifier>
                                          <ColumnReference Database=""[DacPac]"" Schema=""[dbo]"" Table=""[table_one]"" Column=""a"" />
                                        </Identifier>
                                      </ScalarOperator>
                                      <ScalarOperator>
                                        <Identifier>
                                          <ColumnReference Database=""[DacPac]"" Schema=""[dbo]"" Table=""[table_one]"" Column=""a"" />
                                        </Identifier>
                                      </ScalarOperator>
                                    </Compare>
                                  </ScalarOperator>
                                </Predicate>
                                <RelOp AvgRowSize=""19"" EstimateCPU=""0.0001581"" EstimateIO=""0.003125"" EstimateRebinds=""0"" EstimateRewinds=""0"" EstimateRows=""1"" LogicalOp=""Table Scan"" NodeId=""5"" Parallel=""false"" PhysicalOp=""Table Scan"" EstimatedTotalSubtreeCost=""0.0032831"" TableCardinality=""0"">
                                  <OutputList>
                                    <ColumnReference Database=""[DacPac]"" Schema=""[dbo]"" Table=""[table_one]"" Column=""a"" />
                                  </OutputList>
                                  <TableScan Ordered=""false"" ForcedIndex=""false"" NoExpandHint=""false"">
                                    <DefinedValues>
                                      <DefinedValue>
                                        <ColumnReference Database=""[DacPac]"" Schema=""[dbo]"" Table=""[table_one]"" Column=""a"" />
                                      </DefinedValue>
                                    </DefinedValues>
                                    <Object Database=""[DacPac]"" Schema=""[dbo]"" Table=""[table_one]"" TableReferenceId=""1"" IndexKind=""Heap"" />
                                  </TableScan>
                                </RelOp>
                                <RelOp AvgRowSize=""11"" EstimateCPU=""0.0001581"" EstimateIO=""0.003125"" EstimateRebinds=""0"" EstimateRewinds=""0"" EstimateRows=""1"" LogicalOp=""Table Scan"" NodeId=""6"" Parallel=""false"" PhysicalOp=""Table Scan"" EstimatedTotalSubtreeCost=""0.0032831"" TableCardinality=""0"">
                                  <OutputList>
                                    <ColumnReference Database=""[DacPac]"" Schema=""[dbo]"" Table=""[table_one]"" Column=""a"" />
                                  </OutputList>
                                  <TableScan Ordered=""false"" ForcedIndex=""false"" NoExpandHint=""false"">
                                    <DefinedValues>
                                      <DefinedValue>
                                        <ColumnReference Database=""[DacPac]"" Schema=""[dbo]"" Table=""[table_one]"" Column=""a"" />
                                      </DefinedValue>
                                    </DefinedValues>
                                    <Object Database=""[DacPac]"" Schema=""[dbo]"" Table=""[table_one]"" TableReferenceId=""2"" IndexKind=""Heap"" />
                                    <Predicate>
                                      <ScalarOperator ScalarString=""[DacPac].[dbo].[table_one].[a]&lt;&gt;(32)"">
                                        <Compare CompareOp=""NE"">
                                          <ScalarOperator>
                                            <Identifier>
                                              <ColumnReference Database=""[DacPac]"" Schema=""[dbo]"" Table=""[table_one]"" Column=""a"" />
                                            </Identifier>
                                          </ScalarOperator>
                                          <ScalarOperator>
                                            <Const ConstValue=""(32)"" />
                                          </ScalarOperator>
                                        </Compare>
                                      </ScalarOperator>
                                    </Predicate>
                                  </TableScan>
                                </RelOp>
                              </NestedLoops>
                            </RelOp>
                          </Top>
                        </RelOp>
                      </Spool>
                    </RelOp>
                  </ComputeScalar>
                </RelOp>
              </Update>
            </RelOp>
          </QueryPlan>
        </StmtSimple>
      </Statements>
    </Batch>
  </BatchSequence>
</ShowPlanXML>";

        #endregion
    }
}
