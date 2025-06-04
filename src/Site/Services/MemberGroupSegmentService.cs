using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using UmbracoMemberGroupSegments.Extensions;

namespace UmbracoMemberGroupSegments.Services;

public class MemberGroupSegmentService : ISegmentService
{
    private readonly IMemberGroupService _memberGroupService;

    public MemberGroupSegmentService(IMemberGroupService memberGroupService)
        => _memberGroupService = memberGroupService;

    public async Task<Attempt<PagedModel<Segment>?, SegmentOperationStatus>> GetPagedSegmentsAsync(int skip = 0, int take = 100)
    {
        var allMemberGroups = (await _memberGroupService.GetAllAsync()).ToArray();
        return Attempt.SucceedWithStatus<PagedModel<Segment>?, SegmentOperationStatus>
        (
            SegmentOperationStatus.Success,
            new PagedModel<Segment>
            {
                Total = allMemberGroups.Length,
                Items = allMemberGroups
                    .Skip(skip)
                    .Take(take)
                    .Select(memberGroup => new Segment
                    {
                        Alias = memberGroup.Name!.AsSegmentAlias(),
                        Name = memberGroup.Name!,
                    })
            }
        );
    }
}